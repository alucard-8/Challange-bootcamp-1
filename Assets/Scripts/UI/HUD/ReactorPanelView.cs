using System.Collections.Generic;
using Soulbreak.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class ReactorPanelView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private WaveManager waves;

        [SerializeField] private Text waveText;
        [SerializeField] private Text subText;
        [SerializeField] private Text healthText;
        [SerializeField] private RectTransform healthFill;
        [SerializeField] private Image shieldPipTemplate;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private readonly List<Image> shieldPips = new List<Image>();

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
            healthText.text = string.Format(S.hudCoreHealth, Lex.T("core", "CORE"), cur, max);
            UIFactory.SetBar(healthFill, max > 0 ? (float)cur / max : 0f);
        }

        private void HandleShield(int cur, int max)
        {
            UIFactory.Pips(shieldPipTemplate, shieldPips, max, 8f);
            for (int i = 0; i < shieldPips.Count; i++) shieldPips[i].color = i < cur ? HudColors.Cyan : HudColors.Dim;
        }

        private void HandleWaveStarted(int wave, bool boss)
        {
            waveText.text = string.Format(S.hudWave, wave);
            subText.text = boss ? S.hudBossWave : Lex.T("defend", "Defend the reactor");
        }

        private void HandleWaveCleared(int wave) => subText.text = S.hudWaveClearedShop;
    }
}
