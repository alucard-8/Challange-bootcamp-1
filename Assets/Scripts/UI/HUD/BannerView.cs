using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class BannerView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private WaveManager waves;
        [SerializeField] private NovaAbility nova;

        [SerializeField] private Text text;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private float timer, punch;


        private void OnEnable()
        {
            waves.OnWaveStarted += HandleWaveStarted;
            waves.OnWaveCleared += HandleWaveCleared;
            nova.OnReady += HandleNovaReady;
        }

        private void OnDisable()
        {
            waves.OnWaveStarted -= HandleWaveStarted;
            waves.OnWaveCleared -= HandleWaveCleared;
            nova.OnReady -= HandleNovaReady;
        }

        private void HandleWaveStarted(int wave, bool boss) =>
            Show(boss ? string.Format(S.hudWaveBoss, wave) : string.Format(S.hudWave, wave), boss ? HudColors.Red : Color.white, 2f);

        private void HandleWaveCleared(int wave) => Show(S.hudWaveCleared, HudColors.Soul, 1.5f);
        private void HandleNovaReady() => Show(S.hudNovaReady, HudColors.Cyan, 1f);

        private void Show(string message, Color color, float duration)
        {
            text.text = message;
            text.color = color;
            timer = duration;
            group.alpha = 1f;
            punch = 1f;
        }

        private void Update()
        {
            if (timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(timer * 2f);
            }
            punch = Mathf.Max(0f, punch - Time.unscaledDeltaTime * 3f);
            text.rectTransform.localScale = Vector3.one * (1f + 0.35f * punch * punch);
        }
    }
}
