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

        private Image flash;
        private float alpha;

        private void Awake()
        {
            flash = UIFactory.Panel(hud.Root, new Color(1, 1, 1, 0), "ImpactFlash");
            flash.rectTransform.Fill();
            flash.transform.SetAsFirstSibling();
        }

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
