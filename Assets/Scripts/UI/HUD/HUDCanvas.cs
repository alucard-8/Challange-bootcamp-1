using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.UI
{
    public static class HudColors
    {
        public static readonly Color Soul = new Color(0.78f, 0.5f, 1f);
        public static readonly Color Cyan = new Color(0.45f, 0.85f, 1f);
        public static readonly Color Red = new Color(1f, 0.35f, 0.4f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
        public static readonly Color Dim = new Color(1f, 1f, 1f, 0.15f);
        public static readonly Color Panel = new Color(0.05f, 0.03f, 0.1f, 0.65f);
    }

    // Owns the shared HUD canvas that the small HUD views build into, and fades the HUD out
    // while the shop is open. Each piece of the HUD is its own view class (single responsibility).
    [DefaultExecutionOrder(-10)]
    public class HUDCanvas : MonoBehaviour
    {
        [SerializeField] private GameManager game;

        public RectTransform Root { get; private set; }
        private CanvasGroup group;

        private void Awake()
        {
            Root = (RectTransform)UIFactory.Canvas("HUD Canvas", transform, 0).transform;
            group = Root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            UIFactory.Label(Root, "LMB shoot   R reload   SPACE nova   ESC pause", 20, TextAnchor.LowerLeft, new Color(1, 1, 1, 0.35f))
                .rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(26, 26), new Vector2(700, 30));
        }

        private void Update()
        {
            float target = game.State == GameState.Shop ? 0f : 1f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.unscaledDeltaTime * 4f);
        }
    }
}
