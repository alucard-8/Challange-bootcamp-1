using Soulbreak.Fx;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Short white impact flash over the whole screen, driven by HitStop impacts.
    public class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private HitStop hitStop;

        [SerializeField] private Image flash;
        private float alpha;

        private void OnEnable() => hitStop.OnImpact += HandleImpact;
        private void OnDisable() => hitStop.OnImpact -= HandleImpact;

        private void HandleImpact(float strength) => alpha = Mathf.Max(alpha, 0.22f * strength);

        private void Update()
        {
            if (alpha <= 0f) return;
            alpha = Mathf.Max(0f, alpha - Time.unscaledDeltaTime * 2.5f);
            flash.color = new Color(1f, 0.95f, 1f, alpha);
        }
    }
}
