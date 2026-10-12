using System.Collections.Generic;
using Soulbreak.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Top-left panel: wave number + reactor core health bar + shield pips.
    public class ReactorPanelView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private WaveManager waves;

        private Text waveText, subText, healthText;
        private RectTransform healthFill, shieldRow;
        private readonly List<Image> shieldPips = new List<Image>();

        private void Awake() => Build();

        private void OnEnable()
        {
            reactor.OnHealthChanged += HandleHealth;
            reactor.OnShieldChanged += HandleShield;
            waves.OnWaveStarted += HandleWaveStarted;
            waves.OnWaveCleared += HandleWaveCleared;
        }

        private void OnDisable()
        {
            reactor.OnHealthChanged -= HandleHealth;
            reactor.OnShieldChanged -= HandleShield;
            waves.OnWaveStarted -= HandleWaveStarted;
            waves.OnWaveCleared -= HandleWaveCleared;
        }

        private void HandleHealth(int cur, int max)
        {
            healthText.text = $"CORE  {cur}/{max}";
            UIFactory.SetBar(healthFill, max > 0 ? (float)cur / max : 0f);
        }

        private void HandleShield(int cur, int max)
        {
            UIFactory.Pips(shieldRow, shieldPips, max, 26f, 8f, HudColors.Dim);
            for (int i = 0; i < shieldPips.Count; i++) shieldPips[i].color = i < cur ? HudColors.Cyan : HudColors.Dim;
        }

        private void HandleWaveStarted(int wave, bool boss)
        {
            waveText.text = $"WAVE {wave}";
            subText.text = boss ? "<color=#FF5566>BOSS WAVE</color>" : "Defend the reactor";
        }

        private void HandleWaveCleared(int wave) => subText.text = "Wave cleared - shopping";

        private void Build()
        {
            var tl = UIFactory.StyledPanel(hud.Root, HudColors.Panel, null, "WavePanel").rectTransform
                .Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(430, 210));
            waveText = UIFactory.Label(tl, "WAVE 1", 54, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            waveText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -10), new Vector2(400, 64));
            subText = UIFactory.Label(tl, "Get ready", 22, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.6f));
            subText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -72), new Vector2(400, 30));
            healthText = UIFactory.Label(tl, "CORE", 22, TextAnchor.MiddleLeft, Color.white, FontStyle.Bold);
            healthText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -110), new Vector2(400, 28));
            healthFill = UIFactory.Bar(tl, new Color(0, 0, 0, 0.6f), HudColors.Red, "HealthBar");
            healthFill.parent.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -140), new Vector2(386, 18));
            UIFactory.Label(tl, "SHIELD", 22, TextAnchor.MiddleLeft, HudColors.Cyan, FontStyle.Bold)
                .rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -174), new Vector2(120, 28));
            shieldRow = UIFactory.Rect("ShieldPips", tl).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, -174), new Vector2(278, 28));
        }
    }
}
