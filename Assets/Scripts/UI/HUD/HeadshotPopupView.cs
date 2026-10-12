using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.UI
{
    // "HEADSHOT" text that pops where the headshot landed.
    public class HeadshotPopupView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private Vector2EventChannel headshots;
        [SerializeField] private FloatingText popupTemplate;

        private void OnEnable() => headshots.OnRaised += HandleHeadshot;
        private void OnDisable() => headshots.OnRaised -= HandleHeadshot;

        private void HandleHeadshot(Vector2 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 local);
            var t = UIFactory.Spawn(popupTemplate, hud.Root);
            ((RectTransform)t.transform).anchoredPosition = local + Vector2.up * 30f;
        }
    }
}
