using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.UI
{
    // "HEADSHOT" text that pops where the headshot landed.
    public class HeadshotPopupView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private Vector2EventChannel headshots;

        private void OnEnable() => headshots.OnRaised += HandleHeadshot;
        private void OnDisable() => headshots.OnRaised -= HandleHeadshot;

        private void HandleHeadshot(Vector2 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 local);
            var t = UIFactory.Label(hud.Root, "HEADSHOT", 34, TextAnchor.MiddleCenter, new Color(1f, 0.35f, 0.3f), FontStyle.Bold, "Headshot");
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.sizeDelta = new Vector2(300f, 60f);
            t.rectTransform.anchoredPosition = local + Vector2.up * 30f;
            t.gameObject.AddComponent<FloatingText>();
        }
    }
}
