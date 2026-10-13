using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.UI
{
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

    [DefaultExecutionOrder(-10)]
    public class HUDCanvas : MonoBehaviour
    {
        [SerializeField] private GameManager game;

        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup group;

        public RectTransform Root => root;

        private void Update()
        {
            float target = game.State == GameState.Combat ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4f);
        }
    }
}
