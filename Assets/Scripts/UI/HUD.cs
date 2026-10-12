using System.Collections.Generic;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Pure listener: subscribes to every system's events and redraws. No gameplay logic here.
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private ComboSystem combo;
        [SerializeField] private WaveManager waves;
        [SerializeField] private GameManager gameManager;

        private static readonly Color Soul = new Color(0.78f, 0.5f, 1f);
        private static readonly Color Cyan = new Color(0.45f, 0.85f, 1f);
        private static readonly Color Red = new Color(1f, 0.35f, 0.4f);
        private static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
        private static readonly Color Dim = new Color(1f, 1f, 1f, 0.15f);

        private RectTransform root;
        private Text waveText, waveSubText, healthText, shieldText, soulsText, reloadText, novaText, comboText, bannerText, gameOverText;
        private RectTransform healthFill, reloadFill, novaFill, comboFill, ammoRow, shieldRow;
        private CanvasGroup bannerGroup, gameOverGroup;
        private readonly List<Image> ammoPips = new List<Image>();
        private readonly List<Image> shieldPips = new List<Image>();
        private float bannerTimer;
        private int displayedSouls, targetSouls;

        private void Awake() => Build();

        private void OnEnable()
        {
            reactor.OnHealthChanged += HandleHealth;
            reactor.OnShieldChanged += HandleShield;
            wallet.OnSoulsChanged += HandleSouls;
            wallet.OnSoulsGained += HandleSoulsGained;
            ammo.OnAmmoChanged += HandleAmmo;
            nova.OnReady += HandleNovaReady;
            combo.OnComboChanged += HandleCombo;
            waves.OnWaveStarted += HandleWaveStarted;
            waves.OnWaveCleared += HandleWaveCleared;
            gameManager.OnStateChanged += HandleState;
        }

        private void OnDisable()
        {
            reactor.OnHealthChanged -= HandleHealth;
            reactor.OnShieldChanged -= HandleShield;
            wallet.OnSoulsChanged -= HandleSouls;
            wallet.OnSoulsGained -= HandleSoulsGained;
            ammo.OnAmmoChanged -= HandleAmmo;
            nova.OnReady -= HandleNovaReady;
            combo.OnComboChanged -= HandleCombo;
            waves.OnWaveStarted -= HandleWaveStarted;
            waves.OnWaveCleared -= HandleWaveCleared;
            gameManager.OnStateChanged -= HandleState;
        }

        // ---------- Event handlers ----------
        private void HandleHealth(int cur, int max)
        {
            healthText.text = $"CORE  {cur}/{max}";
            UIFactory.SetBar(healthFill, max > 0 ? (float)cur / max : 0f);
        }

        private void HandleShield(int cur, int max)
        {
            shieldText.text = "SHIELD";
            Pips(shieldRow, shieldPips, max, 26f, 8f);
            for (int i = 0; i < shieldPips.Count; i++) shieldPips[i].color = i < cur ? Cyan : Dim;
        }

        private void HandleSouls(int souls) => targetSouls = souls;

        private void HandleSoulsGained(int amount, Vector3 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 local);
            var t = UIFactory.Label(root, $"+{amount}", amount >= 30 ? 40 : 28, TextAnchor.MiddleCenter, Soul, FontStyle.Bold, "SoulPopup");
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.sizeDelta = new Vector2(200f, 60f);
            t.rectTransform.anchoredPosition = local;
            t.gameObject.AddComponent<FloatingText>();
        }

        private void HandleAmmo(int cur, int max)
        {
            Pips(ammoRow, ammoPips, max, 18f, 6f, 46f);
            for (int i = 0; i < ammoPips.Count; i++) ammoPips[i].color = i < cur ? Gold : Dim;
        }

        private void HandleNovaReady() => ShowBanner("NOVA READY", Cyan, 1f);

        private void HandleCombo(int count, float mult)
        {
            comboText.text = count >= 2 ? $"COMBO {count}   <color=#{UIFactory.Hex(Gold)}>x{mult:0.00}</color>" : "";
        }

        private void HandleWaveStarted(int wave, bool boss)
        {
            waveText.text = $"WAVE {wave}";
            waveSubText.text = boss ? "<color=#FF5566>BOSS WAVE</color>" : "Defend the reactor";
            ShowBanner(boss ? $"WAVE {wave}\n<size=40>THE BOSS APPROACHES</size>" : $"WAVE {wave}", boss ? Red : Color.white, 2f);
        }

        private void HandleWaveCleared(int wave)
        {
            waveSubText.text = "Wave cleared - shopping";
            ShowBanner("WAVE CLEARED", Soul, 1.5f);
        }

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) return;
            gameOverGroup.alpha = 1f;
            gameOverText.text =
                "<size=90><b>REACTOR DESTROYED</b></size>\n\n" +
                $"Waves cleared  <b>{gameManager.WavesCleared}</b>\n" +
                $"Time survived  <b>{gameManager.SurvivalTime:0}s</b>\n" +
                $"Souls banked  <b>{wallet.Souls}</b>\n\n" +
                "<size=34>Press R to restart</size>";
        }

        // ---------- Per-frame (timers only) ----------
        private void Update()
        {
            // Rolling soul counter
            if (displayedSouls != targetSouls)
            {
                int step = Mathf.Max(1, Mathf.Abs(targetSouls - displayedSouls) / 8);
                displayedSouls += displayedSouls < targetSouls ? Mathf.Min(step, targetSouls - displayedSouls)
                                                               : -Mathf.Min(step, displayedSouls - targetSouls);
                soulsText.text = $"{displayedSouls}";
            }

            reloadText.text = ammo.IsReloading ? "RELOADING" : "R / RMB to reload";
            reloadText.color = ammo.IsReloading ? Gold : new Color(1f, 1f, 1f, 0.35f);
            UIFactory.SetBar(reloadFill, ammo.IsReloading ? ammo.ReloadProgress : 0f);

            float novaReady = nova.Cooldown > 0f ? 1f - nova.CooldownRemaining / nova.Cooldown : 1f;
            UIFactory.SetBar(novaFill, nova.IsReady ? 1f : novaReady);
            novaText.text = nova.IsReady ? "NOVA  <b>[SPACE]</b>" : $"NOVA  {nova.CooldownRemaining:0.0}s";
            novaText.color = nova.IsReady ? Cyan : new Color(1f, 1f, 1f, 0.5f);

            UIFactory.SetBar(comboFill, combo.Count >= 2 ? combo.TimeLeft01 : 0f);

            if (bannerTimer > 0f)
            {
                bannerTimer -= Time.unscaledDeltaTime;
                bannerGroup.alpha = Mathf.Clamp01(bannerTimer * 2f);
            }
        }

        private void ShowBanner(string text, Color color, float duration)
        {
            bannerText.text = text;
            bannerText.color = color;
            bannerTimer = duration;
            bannerGroup.alpha = 1f;
        }

        private static void Pips(RectTransform row, List<Image> pips, int count, float size, float gap, float height = 0f)
        {
            if (pips.Count == count) return;
            foreach (var p in pips) Destroy(p.gameObject);
            pips.Clear();
            float h = height > 0f ? height : size;
            float total = count * size + (count - 1) * gap;
            for (int i = 0; i < count; i++)
            {
                var img = UIFactory.Panel(row, Dim, "Pip");
                img.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-total / 2f + size / 2f + i * (size + gap), 0f), new Vector2(size, h));
                pips.Add(img);
            }
        }

        // ---------- Layout ----------
        private void Build()
        {
            root = (RectTransform)UIFactory.Canvas("HUD Canvas", transform, 0).transform;
            var white = Color.white;
            var panel = new Color(0.05f, 0.03f, 0.1f, 0.65f);

            // Top-left: wave + core
            var tl = UIFactory.Panel(root, panel, "WavePanel").rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(430, 210));
            waveText = UIFactory.Label(tl, "WAVE 1", 54, TextAnchor.UpperLeft, white, FontStyle.Bold);
            waveText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -10), new Vector2(400, 64));
            waveSubText = UIFactory.Label(tl, "Get ready", 22, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.6f));
            waveSubText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -72), new Vector2(400, 30));
            healthText = UIFactory.Label(tl, "CORE", 22, TextAnchor.MiddleLeft, white, FontStyle.Bold);
            healthText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -110), new Vector2(400, 28));
            var hb = UIFactory.Bar(tl, new Color(0, 0, 0, 0.6f), Red, "HealthBar");
            hb.parent.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -140), new Vector2(386, 18));
            healthFill = hb;
            shieldText = UIFactory.Label(tl, "SHIELD", 22, TextAnchor.MiddleLeft, Cyan, FontStyle.Bold);
            shieldText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -174), new Vector2(120, 28));
            shieldRow = UIFactory.Rect("ShieldPips", tl).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, -174), new Vector2(278, 28));

            // Top-right: souls
            var tr = UIFactory.Panel(root, panel, "SoulPanel").rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Vector2(300, 110));
            UIFactory.Label(tr, "SOULS", 22, TextAnchor.UpperRight, new Color(1, 1, 1, 0.6f)).rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -10), new Vector2(260, 28));
            soulsText = UIFactory.Label(tr, "0", 56, TextAnchor.UpperRight, Soul, FontStyle.Bold);
            soulsText.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -36), new Vector2(260, 64));

            // Top-center: combo + banner
            comboText = UIFactory.Label(root, "", 34, TextAnchor.MiddleCenter, white, FontStyle.Bold);
            comboText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(600, 44));
            var cb = UIFactory.Bar(root, new Color(0, 0, 0, 0), Gold, "ComboBar");
            cb.parent.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -78), new Vector2(260, 8));
            comboFill = cb;

            bannerText = UIFactory.Label(root, "", 96, TextAnchor.MiddleCenter, white, FontStyle.Bold, "Banner");
            bannerText.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1400, 260));
            bannerGroup = bannerText.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.alpha = 0f;

            // Bottom-center: ammo
            var bc = UIFactory.Rect("AmmoPanel", root).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(600, 110));
            ammoRow = UIFactory.Rect("AmmoPips", bc).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(600, 50));
            var rb = UIFactory.Bar(bc, new Color(0, 0, 0, 0.5f), Gold, "ReloadBar");
            rb.parent.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(240, 8));
            reloadFill = rb;
            reloadText = UIFactory.Label(bc, "", 20, TextAnchor.MiddleCenter, white, FontStyle.Bold);
            reloadText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -76), new Vector2(400, 28));

            // Bottom-right: nova
            var br = UIFactory.Panel(root, panel, "NovaPanel").rectTransform.Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 24), new Vector2(300, 80));
            novaText = UIFactory.Label(br, "NOVA", 26, TextAnchor.MiddleLeft, Cyan, FontStyle.Bold);
            novaText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -8), new Vector2(270, 34));
            var nb = UIFactory.Bar(br, new Color(0, 0, 0, 0.6f), Cyan, "NovaBar");
            nb.parent.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -50), new Vector2(264, 14));
            novaFill = nb;

            // Bottom-left: controls hint
            UIFactory.Label(root, "LMB shoot   R reload   SPACE nova", 20, TextAnchor.LowerLeft, new Color(1, 1, 1, 0.35f))
                .rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(26, 26), new Vector2(600, 30));

            // Game over
            var go = UIFactory.Panel(root, new Color(0.03f, 0f, 0.05f, 0.85f), "GameOver");
            go.rectTransform.Fill();
            gameOverGroup = go.gameObject.AddComponent<CanvasGroup>();
            gameOverGroup.alpha = 0f;
            gameOverGroup.blocksRaycasts = false;
            gameOverText = UIFactory.Label(go.transform, "", 40, TextAnchor.MiddleCenter, white);
            gameOverText.rectTransform.Fill();
        }
    }
}
