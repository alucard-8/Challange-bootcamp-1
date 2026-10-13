using System;
using Soulbreak.Core;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using Soulbreak.Orbs;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // First-run interactive coach. Each step:
    //   - sets up a little practice moment (drops orbs, charges Nova / Overdrive),
    //   - shows a big key prompt and a marker that tracks the thing to look at in the world,
    //   - waits until the player actually does it (observed through events), then ticks off.
    // T skips. Finishing or skipping turns GameSettings.Tutorial off (Settings can turn it back on).
    // Step copy and chrome live in UIStrings.
    public class TutorialView : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private OverdriveSystem overdrive;
        [SerializeField] private OrbDropper dropper;
        [SerializeField] private EnemyRegistry registry;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private OrbEventChannel orbCollected;

        private enum Look { None, Enemy, Orb, AmmoOrb, Core }

        private class Step
        {
            public string Title, Body, Key;
            public Func<bool> Done;   // null = info step, completes after MinTime
            public float MinTime;     // keeps fast players from skipping the text unread
            public Look Look;
            public Action Enter;      // practice setup
            public Action Tick;       // keeps the practice going (e.g. re-drops orbs)
        }

        private Step[] steps;
        private int index = -1;
        private float stepTime, doneTimer = -1f, slide, retimer;
        private int shots, kills, reloads, orbs, ammoOrbs, novas, overdrives;
        private bool active;

        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup group, markerGroup;
        [SerializeField] private RectTransform card, marker, keyCap;
        [SerializeField] private Text counter, title, body, check, keyText, markerText;
        [SerializeField] private Image progress;
        [SerializeField] private UIConfetti confetti;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private static string At(string[] a, int i) => a != null && i >= 0 && i < a.Length ? a[i] : "";

        private void Awake()
        {
            active = GameSettings.Tutorial;
            if (!active) { enabled = false; return; }

            // Gameplay behaviour lives here; the words come from UIStrings (same order).
            steps = new[]
            {
                new Step { Look = Look.Enemy, MinTime = 1f, Done = () => shots >= 3 },
                new Step { Look = Look.Enemy, MinTime = 1.5f, Done = () => kills >= 3 },
                new Step { Look = Look.Core, MinTime = 9f },
                new Step { MinTime = 1f, Done = () => reloads >= 1 },
                new Step { Look = Look.AmmoOrb, MinTime = 2f,
                    Enter = () => DropAmmo(), Tick = () => { if (ammoOrbs == 0 && Retry(5f)) DropAmmo(); }, Done = () => ammoOrbs >= 1 },
                new Step { Look = Look.Orb, MinTime = 1.5f,
                    Enter = () => DropPickups(), Tick = () => { if (!AnyOrb() && Retry(2f)) DropPickups(); }, Done = () => orbs >= 1 },
                new Step { Look = Look.Enemy, MinTime = 1f,
                    Enter = () => nova.ResetCooldown(), Done = () => novas >= 1 },
                new Step { MinTime = 1f,
                    Enter = () => overdrive.Fill(), Done = () => overdrives >= 1 },
                new Step { MinTime = 7f },
            };
            for (int i = 0; i < steps.Length; i++)
            {
                steps[i].Title = At(S.tutorialTitles, i);
                steps[i].Key = At(S.tutorialKeys, i);
                steps[i].Body = At(S.tutorialBodies, i);
            }
        }

        private bool Retry(float every)
        {
            retimer += Time.deltaTime;
            if (retimer < every) return false;
            retimer = 0f;
            return true;
        }

        private void DropAmmo()
        {
            // from a random point at the edge of the screen, so it visibly travels in
            var cam = Camera.main;
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            float dist = cam != null ? cam.orthographicSize * 0.8f : 4f;
            dropper.SpawnFor(OrbType.Ammo, dir * dist);
        }

        private void DropPickups()
        {
            float a = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            dropper.SpawnFor(OrbType.Soul, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 2.6f);
            dropper.SpawnFor(OrbType.Heal, new Vector2(Mathf.Cos(a + 2.4f), Mathf.Sin(a + 2.4f)) * 2.6f);
        }

        private static bool AnyOrb() => FindAnyObjectByType<Orb>() != null;

        private void OnEnable()
        {
            if (!active) return;
            shooter.OnShotFired += CountShot;
            ammo.OnReloadStarted += CountReload;
            ammo.OnAmmoPickup += CountAmmo;
            nova.OnUsed += CountNova;
            overdrive.OnStarted += CountOverdrive;
            enemyKilled.OnRaised += CountKill;
            orbCollected.OnRaised += CountOrb;
        }

        private void OnDisable()
        {
            if (!active) return;
            shooter.OnShotFired -= CountShot;
            ammo.OnReloadStarted -= CountReload;
            ammo.OnAmmoPickup -= CountAmmo;
            nova.OnUsed -= CountNova;
            overdrive.OnStarted -= CountOverdrive;
            enemyKilled.OnRaised -= CountKill;
            orbCollected.OnRaised -= CountOrb;
        }

        private void CountShot(Vector2 a, Vector2 b, bool c, Weapons.ShotPattern p) => shots++;
        private void CountReload(float d) => reloads++;
        private void CountAmmo() => ammoOrbs++;
        private void CountNova(Vector2 c, float r) => novas++;
        private void CountOverdrive() => overdrives++;
        private void CountKill(Enemy e) => kills++;
        private void CountOrb(Orb o) { if (o.Type != OrbType.Ammo) orbs++; }

        private void Start() => Next();

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool running = index < steps.Length;
            bool combat = game.State == GameState.Combat && Time.timeScale > 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, combat && running ? 1f : 0f, dt * 5f);
            if (!running) return;

            if (InputReader.SkipTutorialPressed()) { Finish(); return; }
            if (game.State == GameState.Shop) { Finish(); return; }   // reaching the shop = seen enough

            var step = steps[index];

            // slide-in + key prompt pulse
            slide = Mathf.MoveTowards(slide, 1f, dt * 3.5f);
            float e = 1f - Mathf.Pow(1f - slide, 3f);
            card.anchoredPosition = new Vector2(Mathf.Lerp(-460f, 24f, e), 0f);
            keyCap.localScale = Vector3.one * (doneTimer >= 0f ? 1f : 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 6f)) * e;
            UpdateMarker(step.Look);

            if (!combat) return;
            stepTime += Time.deltaTime;
            progress.rectTransform.localScale = new Vector3(Mathf.Clamp01(stepTime / step.MinTime), 1f, 1f);
            step.Tick?.Invoke();

            if (doneTimer < 0f)
            {
                if (stepTime >= step.MinTime && (step.Done == null || step.Done()))
                {
                    doneTimer = 1f;
                    check.text = S.tutorialNice;
                    check.color = T.good;
                    keyText.text = S.tutorialDone;
                    keyCap.GetComponent<Image>().color = T.keyCapDone;
                    Audio.UIAudio.Play(Audio.Sfx.Buy);
                    UIConfetti.Burst(confetti, (Vector2)root.InverseTransformPoint(card.position) + new Vector2(210f, 0f), T.confettiGold, 18, 420f);
                }
            }
            else if ((doneTimer -= dt) <= 0f) Next();
        }

        // Marker: a pulsing ring + label that follows whatever this step is about.
        private void UpdateMarker(Look look)
        {
            Vector2? world = null;
            string label = "";
            switch (look)
            {
                case Look.Enemy: world = NearestEnemy(); label = S.tutorialTarget; break;
                case Look.Orb: world = NearestOrb(false); label = S.tutorialShoot; break;
                case Look.AmmoOrb: world = NearestOrb(true); label = S.tutorialAmmo; break;
                case Look.Core: world = Vector2.zero; label = S.tutorialRing; break;
            }

            var cam = Camera.main;
            bool show = world.HasValue && cam != null && doneTimer < 0f;
            markerGroup.alpha = Mathf.MoveTowards(markerGroup.alpha, show ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            if (!show) return;

            Vector2 screen = cam.WorldToScreenPoint(world.Value);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local);
            marker.anchoredPosition = Vector2.Lerp(marker.anchoredPosition, local, markerGroup.alpha < 0.05f ? 1f : Time.unscaledDeltaTime * 14f);
            float pulse = 1f + 0.15f * Mathf.Sin(Time.unscaledTime * 8f);
            marker.localScale = Vector3.one * pulse;
            marker.Rotate(0f, 0f, 90f * Time.unscaledDeltaTime);
            markerText.text = label;
            markerText.rectTransform.anchoredPosition = local + new Vector2(0f, 62f);
        }

        private Vector2? NearestEnemy()
        {
            if (registry == null) return null;
            Vector2? best = null;
            float bestD = float.MaxValue;
            foreach (var en in registry.Alive)
            {
                if (en == null) continue;
                float d = ((Vector2)en.transform.position).sqrMagnitude;   // closest to the core = most urgent
                if (d < bestD) { bestD = d; best = en.transform.position; }
            }
            return best;
        }

        private static Vector2? NearestOrb(bool ammoOnly)
        {
            foreach (var o in FindObjectsByType<Orb>())
                if ((o.Type == OrbType.Ammo) == ammoOnly) return o.transform.position;
            return null;
        }

        private void Next()
        {
            index++;
            if (index >= steps.Length) { Finish(); return; }
            var s = steps[index];
            counter.text = string.Format(S.tutorialCounter, index + 1, steps.Length);
            title.text = s.Title;
            body.text = s.Body;
            check.text = S.tutorialSkip;
            check.color = T.faintText;
            keyCap.gameObject.SetActive(!string.IsNullOrEmpty(s.Key));
            keyText.text = s.Key ?? "";
            keyCap.GetComponent<Image>().color = T.keyCapBg;
            keyCap.sizeDelta = new Vector2(Mathf.Max(120f, (s.Key ?? "").Length * 20f + 60f), 64f);
            stepTime = 0f; doneTimer = -1f; slide = 0f; retimer = 0f;
            s.Enter?.Invoke();
            if (index > 0) Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        private void Finish()
        {
            index = steps.Length;
            if (GameSettings.Tutorial) GameSettings.Tutorial = false;
        }
    }
}
