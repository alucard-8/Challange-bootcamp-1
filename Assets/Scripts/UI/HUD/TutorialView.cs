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
    //   - plays a visual demo (highlight ring + animated key ring + arrow + ghost hand that taps),
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

        // ---- visual demos (sprites generated once, elements built under the tutorial content group) ----
        private RectTransform demoSpace;
        private Image focusRing, keyRing, arrow, hand, ripple;
        private static Sprite ringSprite, arrowSprite, handSprite;

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

            BuildDemo();
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
            UpdateDemo(step);

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
            Vector2? world = WorldFocus(look);
            string label = look switch
            {
                Look.Enemy => S.tutorialTarget,
                Look.Orb => S.tutorialShoot,
                Look.AmmoOrb => S.tutorialAmmo,
                Look.Core => S.tutorialRing,
                _ => "",
            };

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

        // ---------- visual demos ----------
        private void BuildDemo()
        {
            if (marker == null) return;

            // Put the demo layer beside the marker inside the tutorial content group, so it fades
            // with the whole HUD (the `group` CanvasGroup) but not with the marker's own fade.
            var markerGroupRt = marker.parent;
            var content = markerGroupRt != null && markerGroupRt.parent != null ? markerGroupRt.parent : markerGroupRt;

            var go = new GameObject("Demo", typeof(RectTransform));
            demoSpace = go.GetComponent<RectTransform>();
            demoSpace.SetParent(content, false);
            demoSpace.anchorMin = Vector2.zero;
            demoSpace.anchorMax = Vector2.one;
            demoSpace.offsetMin = Vector2.zero;
            demoSpace.offsetMax = Vector2.zero;
            demoSpace.pivot = new Vector2(0.5f, 0.5f);
            demoSpace.anchoredPosition = Vector2.zero;
            demoSpace.SetAsLastSibling();

            focusRing = MakeIcon("FocusRing", RingSprite, 150f);
            keyRing = MakeIcon("KeyRing", RingSprite, 130f);
            arrow = MakeIcon("Arrow", ArrowSprite, 40f);
            hand = MakeIcon("Hand", HandSprite, 46f);
            ripple = MakeIcon("Ripple", RingSprite, 80f);
        }

        private Image MakeIcon(string name, Sprite sprite, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(demoSpace, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            img.color = new Color(1f, 1f, 1f, 0f);
            return img;
        }

        private void UpdateDemo(Step step)
        {
            if (focusRing == null) return;

            bool live = doneTimer < 0f;
            Vector2? focus = live ? FocusLocal(step.Look) : null;
            bool hasKey = live && !string.IsNullOrEmpty(step.Key);
            Vector2 keyLocal = UILocal(keyCap);
            Vector2 cardLocal = UILocal(card);
            float now = Time.unscaledTime;

            // Animated key ring: a golden halo pulsing around the key cap.
            keyRing.enabled = hasKey;
            if (hasKey)
            {
                keyRing.rectTransform.position = keyCap.position;
                keyRing.rectTransform.sizeDelta = keyCap.rect.size + new Vector2(24f, 24f);
                float kp = 0.5f + 0.5f * Mathf.Sin(now * 6f);
                keyRing.rectTransform.localScale = Vector3.one * (1f + 0.05f * kp);
                keyRing.color = Fade(T.hudGold, 0.25f + 0.5f * kp);
            }

            // Highlight ring: pulses on whatever the step is about in the world.
            focusRing.enabled = focus.HasValue;
            if (focus.HasValue)
            {
                focusRing.rectTransform.anchoredPosition = focus.Value;
                float fp = 0.5f + 0.5f * Mathf.Sin(now * 7f);
                focusRing.rectTransform.localScale = Vector3.one * (1f + 0.2f * fp);
                focusRing.color = Fade(T.accent, 0.45f + 0.4f * fp);
            }

            // Ghost hand: travels from the key (or card) to the target, then taps it.
            Vector2 from = hasKey ? keyLocal : cardLocal;
            Vector2 to = focus ?? from;
            const float period = 1.9f;
            float ph = Mathf.Repeat(now, period) / period;
            float travel = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(ph / 0.7f));
            bool tapping = ph > 0.7f && focus.HasValue;
            float tap = tapping ? Mathf.Sin(Mathf.Clamp01((ph - 0.7f) / 0.3f) * Mathf.PI) : 0f;

            hand.enabled = live;
            hand.rectTransform.anchoredPosition = Vector2.Lerp(from, to, travel);
            hand.rectTransform.localScale = Vector3.one * (1f + 0.3f * tap);
            hand.color = Fade(T.flashColor, live ? 0.9f : 0f);

            // Arrow: points from the key/card toward the target; bobs above the key when there is none.
            arrow.enabled = live && (focus.HasValue || hasKey);
            if (focus.HasValue)
            {
                arrow.rectTransform.anchoredPosition = Vector2.Lerp(from, to, 0.45f);
                Vector2 dir = to - from;
                arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
                arrow.rectTransform.sizeDelta = new Vector2(40f, 40f);
                arrow.color = Fade(T.hudGold, 0.65f + 0.3f * Mathf.Sin(now * 6f));
            }
            else if (hasKey)
            {
                arrow.rectTransform.anchoredPosition = keyLocal + new Vector2(0f, 92f + 7f * Mathf.Sin(now * 5f));
                arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                arrow.rectTransform.sizeDelta = new Vector2(36f, 36f);
                arrow.color = Fade(T.hudGold, 0.8f);
            }

            // Tap ripple at the target.
            if (tapping)
            {
                float r = Mathf.Clamp01((ph - 0.7f) / 0.3f);
                ripple.enabled = true;
                ripple.rectTransform.anchoredPosition = to;
                ripple.rectTransform.localScale = Vector3.one * (0.4f + 1.8f * r);
                ripple.color = Fade(T.gold, (1f - r) * 0.75f);
            }
            else ripple.enabled = false;
        }

        private Vector2? WorldFocus(Look look)
        {
            switch (look)
            {
                case Look.Enemy: return NearestEnemy();
                case Look.Orb: return NearestOrb(false);
                case Look.AmmoOrb: return NearestOrb(true);
                case Look.Core: return Vector2.zero;
            }
            return null;
        }

        // UI rect -> local point in the tutorial canvas (root space).
        private Vector2 UILocal(Transform t)
            => LocalInRoot(RectTransformUtility.WorldToScreenPoint(null, t.position)) ?? Vector2.zero;

        // World point -> local point in the tutorial canvas (root space), through the game camera.
        private Vector2? FocusLocal(Look look)
        {
            Vector2? w = WorldFocus(look);
            var cam = Camera.main;
            if (!w.HasValue || cam == null) return null;
            return LocalInRoot(cam.WorldToScreenPoint(w.Value));
        }

        private Vector2? LocalInRoot(Vector2 screen)
        {
            if (root == null) return null;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out var local);
            return local;
        }

        private static Color Fade(Color c, float a) => new Color(c.r, c.g, c.b, a);

        private static Sprite RingSprite => ringSprite != null ? ringSprite : (ringSprite = MakeSprite(64, RingAlpha));
        private static Sprite ArrowSprite => arrowSprite != null ? arrowSprite : (arrowSprite = MakeSprite(64, ArrowAlpha));
        private static Sprite HandSprite => handSprite != null ? handSprite : (handSprite = MakeSprite(64, HandAlpha));

        // Generates a small white RGBA sprite (tinted through Image.color) from a coverage function.
        private static Sprite MakeSprite(int size, Func<float, float, float> alpha)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = Mathf.Clamp01(alpha((x + 0.5f) / size, (y + 0.5f) / size));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            sp.hideFlags = HideFlags.HideAndDontSave;
            return sp;
        }

        private static float RingAlpha(float u, float v)
        {
            float dx = u - 0.5f, dy = v - 0.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            return 1f - Mathf.Clamp01(Mathf.Abs(d - 0.40f) / 0.07f);
        }

        private static float ArrowAlpha(float u, float v)
        {
            const float apexY = 0.94f, baseY = 0.10f;
            if (v < baseY || v > apexY) return 0f;
            float t = (apexY - v) / (apexY - baseY);       // 0 at apex -> 1 at base
            float half = Mathf.Lerp(0.03f, 0.44f, t);
            return 1f - Mathf.Clamp01((Mathf.Abs(u - 0.5f) - half) / 0.03f);
        }

        // Classic mouse-cursor silhouette, y-up.
        private static readonly float[] CursorPoly =
        {
            0.30f, 0.95f, 0.30f, 0.12f, 0.46f, 0.30f,
            0.57f, 0.05f, 0.68f, 0.10f, 0.56f, 0.35f, 0.74f, 0.35f,
        };

        private static float HandAlpha(float u, float v)
        {
            float a = 0f;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (InCursor(u + (i - 1) * 0.02f, v + (j - 1) * 0.02f)) a += 1f;
            return a / 9f;
        }

        private static bool InCursor(float u, float v)
        {
            bool inside = false;
            int n = CursorPoly.Length;
            for (int i = 0, j = n - 2; i < n; j = i, i += 2)
            {
                float xi = CursorPoly[i], yi = CursorPoly[i + 1], xj = CursorPoly[j], yj = CursorPoly[j + 1];
                if (((yi > v) != (yj > v)) && (u < (xj - xi) * (v - yi) / (yj - yi) + xi)) inside = !inside;
            }
            return inside;
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
