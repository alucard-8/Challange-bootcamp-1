using System.Collections.Generic;

using Soulbreak.GameFlow;
using Soulbreak.Meta;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MenuAmbience ambience;
        [SerializeField] private AscensionView ascension;
        [SerializeField] private UI.SettingsView settings;
        [SerializeField] private IndexView index;
        [SerializeField] private UI.HowToView howToView;
        [SerializeField] private Meta.CodexCatalog codex;
        [SerializeField] private DifficultyView difficulty;

        [System.Serializable]
        private class Intro
        {
            public RectTransform rt;
            public CanvasGroup group;
            public Vector2 offset;
            public float delay;
            [System.NonSerialized] public Vector2 basePos;
        }

        [SerializeField] private List<Intro> intro = new List<Intro>();
        [SerializeField] private CanvasGroup mainGroup;
        [SerializeField] private RectTransform title;
        [SerializeField] private Text titleLabel, tagLabel, toastText, recordsText;
        [SerializeField] private CanvasGroup toastGroup;
        [SerializeField] private Image flashImage;
        [SerializeField] private Button[] menuButtons;
        [SerializeField] private Button helpButton;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private float toastTimer, flashAlpha;
        private Vector2 titleBase;
        private float time;
        private bool starting;

        private void Awake()
        {
            System.Action[] actions = { OpenPlay, () => ascension.Open(), () => index.Open(), () => settings.Open(), GameManager.Quit };
            for (int i = 0; i < menuButtons.Length && i < actions.Length; i++)
            {
                var action = actions[i];
                menuButtons[i].onClick.AddListener(() => action());
            }
            helpButton.onClick.AddListener(OpenHowTo);
#if UNITY_WEBGL
            menuButtons[menuButtons.Length - 1].gameObject.SetActive(false);
#endif
        }

        private void OnEnable() => MetaProgress.OnChanged += RefreshRecords;
        private void OnDisable() => MetaProgress.OnChanged -= RefreshRecords;

        private void Start()
        {
            Time.timeScale = 1f;
            titleBase = title.anchoredPosition;
            foreach (var e in intro)
            {
                e.basePos = e.rt.anchoredPosition;
                e.rt.anchoredPosition += e.offset;
                e.group.alpha = 0f;
            }
            RefreshRecords();
            SceneFader.FadeFromBlack(1.4f);
        }

        private void Update()
        {
            time += Time.unscaledDeltaTime;
            AnimateToastAndFlash();

            foreach (var e in intro)
            {
                float t = Mathf.Clamp01((time - e.delay) / 0.7f);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                e.group.alpha = ease;
                if (t < 1f) e.rt.anchoredPosition = e.basePos + e.offset * (1f - ease);
            }

            if (time > 1f) title.anchoredPosition = titleBase + Vector2.up * Mathf.Sin(time * 1.3f) * 6f;
        }

        public bool IsHowToOpen => howToView.IsOpen;
        public bool IsAscensionOpen => ascension.IsOpen;
        public bool IsSettingsOpen => settings.IsOpen;
        public bool IsDifficultyOpen => difficulty.IsOpen;
        public void OpenPlay() => difficulty.Open();
        public void ConfirmDifficulty() => difficulty.Confirm();
        public void CloseDifficulty() => difficulty.Close();
        public void CloseSettings() => settings.Close();
        public void CloseAscension() => ascension.Close();
        public void CloseHowTo() => howToView.Close();
        public void OpenHowTo() => howToView.Open();

        public void Play()
        {
            if (starting) return;
            starting = true;
            mainGroup.interactable = false;
            ambience.ZoomIn();
            SceneFader.LoadScene(SceneNames.Game, T.transitionColor, 0.75f);
        }

        private void RefreshRecords()
        {
            if (recordsText == null) return;
            var diff = DifficultyTable.Current;
            recordsText.text = string.Format(S.menuRecords,
                UI.UIFactory.Hex(T.gold),
                MetaProgress.BestScore.ToString("N0"),
                MetaProgress.LastScore.ToString("N0"),
                MetaProgress.LastWave,
                MetaProgress.BestWave,
                MetaProgress.Runs,
                UI.UIFactory.Hex(diff.Color),
                diff.Name);
        }

        public void SetTitle(string titleText, string tagline)
        {
            titleLabel.text = titleText;
            tagLabel.text = tagline;
        }

        public void ShowToast(string message)
        {
            toastText.text = message;
            toastTimer = 2.5f;
        }

        public void Flash()
        {
            if (!Core.GameSettings.ReduceFlashing) flashAlpha = 1f;
        }

        private void AnimateToastAndFlash()
        {
            if (toastTimer > 0f)
            {
                toastTimer -= Time.unscaledDeltaTime;
                toastGroup.alpha = Mathf.Clamp01(toastTimer * 2f);
            }
            if (flashAlpha > 0f)
            {
                flashAlpha = Mathf.Max(0f, flashAlpha - Time.unscaledDeltaTime * 1.5f);
                flashImage.color = new Color(T.flashColor.r, T.flashColor.g, T.flashColor.b, flashAlpha);
            }
        }
    }
}
