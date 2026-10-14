using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
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

        public bool CloseTopPanel()
        {
            if (howTo != null && howTo.IsOpen) { howTo.Close(); return true; }
            if (index != null && index.IsOpen) { index.Close(); return true; }
            if (settings.IsOpen) { settings.Close(); return true; }
            return false;
        }

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
        [SerializeField] private Button[] menuButtons;
        [SerializeField] private Button helpButton;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;
        private const float PanelWidth = 560f;

        private bool quitArmed;
        private float quitTimer;
        private Text quitLabel;
        private string quitLabelDefault;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private void Awake()
        {
            System.Action[] actions =
            {
                () => pause.SetPaused(false),
                () => settings.Open(),
                () => index.Open(),
                () => game.Restart(),
                () => game.ToMainMenu(),
                ConfirmQuit,
            };
            for (int i = 0; i < menuButtons.Length && i < actions.Length; i++)
            {
                var action = actions[i];
                menuButtons[i].onClick.AddListener(() => action());
            }
            helpButton.onClick.AddListener(() => howTo.Open());
            if (menuButtons.Length > 0)
            {
                quitLabel = menuButtons[menuButtons.Length - 1].GetComponentInChildren<Text>();
                if (quitLabel != null) quitLabelDefault = quitLabel.text;
            }
#if UNITY_WEBGL
            menuButtons[menuButtons.Length - 1].gameObject.SetActive(false);
#endif
        }

        private void OnEnable() => pause.OnPauseChanged += HandlePause;
        private void OnDisable() => pause.OnPauseChanged -= HandlePause;

        private void HandlePause(bool paused)
        {
            Audio.UIAudio.Play(paused ? Audio.Sfx.UIOpen : Audio.Sfx.UIClose);
            if (!paused) { CloseAll(); DisarmQuit(); return; }
            statsText.text = string.Format(S.pauseStats, Mathf.Max(1, waves.Wave), Lex.T("souls", "SOULS"), wallet.Souls, game.SurvivalTime);
        }

        private void Update()
        {
            float b = pause.Blend;
            group.alpha = b;
            group.blocksRaycasts = group.interactable = pause.IsPaused;
            dim.color = new Color(T.pauseDim.r, T.pauseDim.g, T.pauseDim.b, T.pauseDim.a * b);

            float slide = 1f - Mathf.Pow(1f - b, 3f);
            panel.anchoredPosition = new Vector2(Mathf.Lerp(-PanelWidth - 40f, 70f, slide), 0f);
            hint.anchoredPosition = new Vector2(Mathf.Lerp(400f, -70f, slide), 70f);

            if (quitArmed)
            {
                quitTimer -= Time.unscaledDeltaTime;
                if (quitTimer <= 0f) DisarmQuit();
            }
        }

        private void ConfirmQuit()
        {
            if (quitArmed) { GameManager.Quit(); return; }
            quitArmed = true;
            quitTimer = 3f;
            if (quitLabel != null) quitLabel.text = S.pauseQuitConfirm;
        }

        private void DisarmQuit()
        {
            quitArmed = false;
            if (quitLabel != null && !string.IsNullOrEmpty(quitLabelDefault)) quitLabel.text = quitLabelDefault;
        }
    }
}
