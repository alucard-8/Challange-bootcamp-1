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
        [SerializeField] private SettingsView settings;
        [SerializeField] private Menu.IndexView index;
        [SerializeField] private HowToView howTo;
        [SerializeField] private Meta.CodexCatalog codex;

        // Esc closes the top-most popup first; returns false when nothing was open.
        public bool CloseTopPanel()
        {
            if (howTo != null && howTo.IsOpen) { howTo.Close(); return true; }
            if (index != null && index.IsOpen) { index.Close(); return true; }
            if (settings.IsOpen) { settings.Close(); return true; }
            return false;
        }

        // Unpausing (or leaving the pause menu any other way) closes every popup.
        private void CloseAll()
        {
            if (howTo != null) howTo.Close();
            if (index != null) index.Close();
            settings.Close();
        }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel, hint;
        [SerializeField] private Text statsText;
        [SerializeField] private Image dim;
        [SerializeField] private Button[] menuButtons;   // resume, settings, index, restart, main menu, quit
        [SerializeField] private Button helpButton;
        private const float PanelWidth = 560f;

        private void Awake()
        {
            System.Action[] actions =
            {
                () => pause.SetPaused(false),
                () => settings.Open(),
                () => index.Open(),
                () => game.Restart(),
                () => game.ToMainMenu(),
                GameManager.Quit,
            };
            for (int i = 0; i < menuButtons.Length && i < actions.Length; i++)
            {
                var action = actions[i];
                menuButtons[i].onClick.AddListener(() => action());
            }
            helpButton.onClick.AddListener(() => howTo.Open());
        }

        private void OnEnable() => pause.OnPauseChanged += HandlePause;
        private void OnDisable() => pause.OnPauseChanged -= HandlePause;

        private void HandlePause(bool paused)
        {
            Audio.UIAudio.Play(paused ? Audio.Sfx.UIOpen : Audio.Sfx.UIClose);
            if (!paused) { CloseAll(); return; }
            statsText.text =
                $"WAVE        <b>{Mathf.Max(1, waves.Wave)}</b>\n" +
                $"{Lex.T("souls", "SOULS"),-12}<b>{wallet.Souls}</b>\n" +
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
    }
}
