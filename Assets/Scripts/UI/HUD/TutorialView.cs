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

        private void Awake()
        {
            active = GameSettings.Tutorial;
            if (!active) { enabled = false; return; }

            steps = new[]
            {
                new Step { Title = "AIM & SHOOT", Key = "LEFT CLICK", Look = Look.Enemy, MinTime = 1f,
                    Body = "Aim with the mouse and fire at the incoming enemies.", Done = () => shots >= 3 },
                new Step { Title = "PROTECT THE CORE", Key = "LEFT CLICK", Look = Look.Enemy, MinTime = 1.5f,
                    Body = "Everything marches on the core in the middle. Destroy <b>3 enemies</b> before they reach it.", Done = () => kills >= 3 },
                new Step { Title = "THE WEAPON RING", Look = Look.Core, MinTime = 9f,
                    Body = "Your shot goes through the <b>ring slot</b> you aim across. <b>Glowing slots</b> hold techniques: they fire once, then reload a random technique you own. The slot count re-rolls every wave (5 to 12)." },
                new Step { Title = "RELOAD", Key = "R  /  RIGHT CLICK", MinTime = 1f,
                    Body = "Empty clips reload on their own, but reloading early saves you in a pinch. <b>Reload now.</b>", Done = () => reloads >= 1 },
                new Step { Title = "AMMO CHARGES", Look = Look.AmmoOrb, MinTime = 2f,
                    Body = "Enemies drop <color=#FFC94D>gold ammo charges</color>. They fly to the core and <b>instantly refill</b> your clip. Watch this one.",
                    Enter = () => DropAmmo(), Tick = () => { if (ammoOrbs == 0 && Retry(5f)) DropAmmo(); }, Done = () => ammoOrbs >= 1 },
                new Step { Title = "PICKUPS", Key = "SHOOT THE ORB", Look = Look.Orb, MinTime = 1.5f,
                    Body = "<color=#C77DFF>Purple orbs</color> are souls, <color=#6CFF9A>green orbs</color> restore shield. <b>Shoot one</b> to collect it.",
                    Enter = () => DropPickups(), Tick = () => { if (!AnyOrb() && Retry(2f)) DropPickups(); }, Done = () => orbs >= 1 },
                new Step { Title = "NOVA", Key = "SPACE", Look = Look.Enemy, MinTime = 1f,
                    Body = "Nova blasts every enemy <b>near the core</b>. Let them come close, then press SPACE.",
                    Enter = () => nova.ResetCooldown(), Done = () => novas >= 1 },
                new Step { Title = "OVERDRIVE", Key = "Q", MinTime = 1f,
                    Body = "Kills fill the overdrive bar. It's full now: press <b>Q</b> to slow time and go berserk.",
                    Enter = () => overdrive.Fill(), Done = () => overdrives >= 1 },
                new Step { Title = "THE SHOP", MinTime = 7f,
                    Body = "Clear the wave to open the shop. Spend souls on perks and techniques and combine them for synergies. Good luck!" },
            };
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
                    check.text = "NICE!";
                    check.color = new Color(0.45f, 1f, 0.6f);
                    keyText.text = "DONE";
                    keyCap.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.35f, 0.95f);
                    Audio.UIAudio.Play(Audio.Sfx.Buy);
                    UIConfetti.Burst(confetti, (Vector2)root.InverseTransformPoint(card.position) + new Vector2(210f, 0f), new Color(1f, 0.79f, 0.3f), 18, 420f);
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
                case Look.Enemy: world = NearestEnemy(); label = "TARGET"; break;
                case Look.Orb: world = NearestOrb(false); label = "SHOOT"; break;
                case Look.AmmoOrb: world = NearestOrb(true); label = "AMMO"; break;
                case Look.Core: world = Vector2.zero; label = "RING"; break;
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
            counter.text = $"TUTORIAL  {index + 1}/{steps.Length}";
            title.text = s.Title;
            body.text = s.Body;
            check.text = "[T] SKIP";
            check.color = new Color(1f, 1f, 1f, 0.4f);
            keyCap.gameObject.SetActive(!string.IsNullOrEmpty(s.Key));
            keyText.text = s.Key ?? "";
            keyCap.GetComponent<Image>().color = new Color(0.08f, 0.05f, 0.15f, 0.95f);
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
