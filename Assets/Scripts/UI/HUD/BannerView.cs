using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Big center announcements that punch in and fade: wave start, boss, wave cleared, nova ready.
    public class BannerView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private WaveManager waves;
        [SerializeField] private NovaAbility nova;

        private Text text;
        private CanvasGroup group;
        private float timer, punch;

        private void Awake() => Build();

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
            Show(boss ? $"WAVE {wave}\n<size=40>THE BOSS APPROACHES</size>" : $"WAVE {wave}", boss ? HudColors.Red : Color.white, 2f);

        private void HandleWaveCleared(int wave) => Show("WAVE CLEARED", HudColors.Soul, 1.5f);
        private void HandleNovaReady() => Show("NOVA READY", HudColors.Cyan, 1f);

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

        private void Build()
        {
            text = UIFactory.Label(hud.Root, "", 96, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, "Banner");
            text.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 220), new Vector2(1400, 260));
            group = text.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
        }
    }
}
