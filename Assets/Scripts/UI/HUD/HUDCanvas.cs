using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.UI
{
    // HUD palette, sourced from the shared UITheme (Resources/UITheme) so the whole look
    // is edited in one place. Kept as named shortcuts for the HUD views.
    public static class HudColors
    {
        private static UITheme T => UIFactory.Theme;
        public static Color Soul => T.soul;
        public static Color Cyan => T.accent;
        public static Color Red => T.danger;
        public static Color Gold => T.hudGold;
        public static Color Dim => T.dim;
        public static Color Panel => T.hudPanel;
    }

    // Owns the shared HUD canvas that the small HUD views build into, and fades the HUD out
    // while the shop is open. Each piece of the HUD is its own view class (single responsibility).
    [DefaultExecutionOrder(-10)]
    public class HUDCanvas : MonoBehaviour
    {
        [SerializeField] private GameManager game;

        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup group;

        public RectTransform Root => root;

        private void Update()
        {
            float target = game.State == GameState.Combat ? 1f : 0f;   // hide during shop and game over
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4f);
        }
    }
}
