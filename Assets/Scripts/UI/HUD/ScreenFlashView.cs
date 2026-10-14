using Soulbreak.Fx;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private HitStop hitStop;

        [SerializeField] private Image flash;
        [SerializeField] private UITheme theme;

        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private float alpha;

        private void OnEnable() => hitStop.OnImpact += HandleImpact;
        private void OnDisable() => hitStop.OnImpact -= HandleImpact;

        private void HandleImpact(float strength) => alpha = Core.GameSettings.ReduceFlashing ? 0f : Mathf.Max(alpha, 0.22f * strength);

        private void Update()
        {
            if (alpha <= 0f) return;
            alpha = Mathf.Max(0f, alpha - Time.unscaledDeltaTime * 2.5f);
            flash.color = new Color(T.screenFlash.r, T.screenFlash.g, T.screenFlash.b, alpha);
        }
    }
}
