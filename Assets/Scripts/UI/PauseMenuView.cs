using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Pause screen: a side panel slides in with a glitching title, run stats and buttons.
    // Animation follows PauseController.Blend; buttons only call PauseController / GameManager.
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private PauseController pause;
        [SerializeField] private GameManager game;
        [SerializeField] private WaveManager waves;
        [SerializeField] private SoulWallet wallet;

        private CanvasGroup group;
        private RectTransform panel, hint;
        private Text statsText;
        private Image dim;
        private const float PanelWidth = 560f;

        private void Awake() => Build();

        private void OnEnable() => pause.OnPauseChanged += HandlePause;
        private void OnDisable() => pause.OnPauseChanged -= HandlePause;

        private void HandlePause(bool paused)
        {
            if (!paused) return;
            statsText.text =
                $"WAVE        <b>{Mathf.Max(1, waves.Wave)}</b>\n" +
                $"SOULS       <b>{wallet.Souls}</b>\n" +
                $"TIME          <b>{game.SurvivalTime:0}s</b>";
        }

        private void Update()
        {
            float b = pause.Blend;
            group.alpha = b;
            group.blocksRaycasts = group.interactable = pause.IsPaused;
            dim.color = new Color(0.02f, 0f, 0.05f, 0.55f * b);

            // Panel slides in from the left with a slight overshoot.
            float slide = 1f - Mathf.Pow(1f - b, 3f);
            panel.anchoredPosition = new Vector2(Mathf.Lerp(-PanelWidth - 40f, 70f, slide), 0f);
            hint.anchoredPosition = new Vector2(Mathf.Lerp(400f, -70f, slide), 70f);
        }

        private void Build()
        {
            var root = (RectTransform)UIFactory.Canvas("Pause Canvas", transform, 20).transform;
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            dim = UIFactory.Panel(root, Color.clear, "Dim");
            dim.rectTransform.Fill();
            dim.raycastTarget = true;

            var theme = UIFactory.Theme;
            panel = UIFactory.StyledPanel(root, new Color(0.04f, 0.02f, 0.09f, 0.92f), theme.accent, "PausePanel", 22f).rectTransform;
            panel.anchorMin = new Vector2(0, 0.5f);
            panel.anchorMax = new Vector2(0, 0.5f);
            panel.pivot = new Vector2(0, 0.5f);
            panel.sizeDelta = new Vector2(PanelWidth, 820f);

            var title = UIFactory.Label(panel, "PAUSED", 110, TextAnchor.UpperLeft, Color.white, FontStyle.Bold, "Title");
            title.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -40), new Vector2(480, 130));
            title.gameObject.AddComponent<GlitchText>();

            var sub = UIFactory.Label(panel, "THE REACTOR HOLDS ITS BREATH", 22, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.45f));
            sub.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -170), new Vector2(480, 30));

            statsText = UIFactory.Label(panel, "", 30, TextAnchor.UpperLeft, theme.soul);
            statsText.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(52, -230), new Vector2(460, 140));

            string[] labels = { "RESUME", "RESTART", "MAIN MENU", "QUIT" };
            System.Action[] actions =
            {
                () => pause.SetPaused(false),
                () => game.Restart(),
                () => game.ToMainMenu(),
                GameManager.Quit,
            };
            for (int i = 0; i < labels.Length; i++)
            {
                var color = i == 0 ? new Color(0.1f, 0.42f, 0.55f, 0.95f) : theme.buttonColor;
                var btn = UIFactory.Button(panel, labels[i], color, actions[i], out _, 32);
                ((RectTransform)btn.transform).Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -400 - i * 95), new Vector2(460, 78));
            }

            hint = UIFactory.Label(root, "ESC  resume\nLMB  shoot through the ring\nR  reload     SPACE  nova", 24, TextAnchor.LowerRight,
                                   new Color(1, 1, 1, 0.5f)).rectTransform;
            hint.anchorMin = hint.anchorMax = hint.pivot = new Vector2(1, 0);
            hint.sizeDelta = new Vector2(600, 120);
        }
    }
}
