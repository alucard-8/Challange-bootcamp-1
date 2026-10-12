using Soulbreak.GameFlow;
using Soulbreak.Player;
using Soulbreak.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Names the shot you're about to fire, just past the needle tip ("CHAIN LIGHTNING").
    // Shows "RELOADING" over an emptied slot; hides over plain laser slots to keep the screen clean.
    public class AimLabelView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private WeaponRing ring;
        [SerializeField] private GameManager game;
        [SerializeField] private Transform core;
        [SerializeField] private float distance = 2.75f;

        [SerializeField] private Text label;
        [SerializeField] private CanvasGroup group;
        private Camera cam;

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || ring.SlotCount == 0) return;

            int slot = ring.GetSlotIndex(shooter.AimDirection);
            bool loading = ring.IsLoading(slot);
            var pattern = ring.GetPatternAt(slot);
            bool show = game.State == GameState.Combat && (loading || pattern != ring.BasicShot);

            group.alpha = Mathf.MoveTowards(group.alpha, show ? 0.9f : 0f, Time.unscaledDeltaTime * 8f);
            if (!show) return;

            label.text = loading ? $"RELOADING  {ring.LoadProgress(slot) * 100f:0}%" : Lex.T(pattern.displayName, pattern.displayName).ToUpper();
            var c = loading ? new Color(1f, 1f, 1f, 0.6f) : pattern.color;
            float m = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            label.color = new Color(c.r / m, c.g / m, c.b / m, 1f);

            Vector2 world = (Vector2)core.position + shooter.AimDirection * distance;
            Vector2 screen = cam.WorldToScreenPoint(world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 local);
            label.rectTransform.anchoredPosition = local;
        }
    }
}
