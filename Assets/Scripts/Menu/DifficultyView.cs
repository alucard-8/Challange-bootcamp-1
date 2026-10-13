using System.Collections.Generic;
using Soulbreak.Audio;
using Soulbreak.Meta;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    // "Choose your difficulty" popup shown when pressing PLAY. Picking a card saves the choice and starts the run.
    public class DifficultyView : MonoBehaviour
    {
        [SerializeField] private MainMenuView menu;

        public bool IsOpen { get; private set; }

        [System.Serializable]
        private class Card
        {
            public Difficulty d;
            public Image card;
            public RectTransform rt;
            public Button button;
        }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button backButton;
        [SerializeField] private List<Card> cards = new List<Card>();
        [SerializeField] private UITheme theme;

        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private float openTime;

        private void Awake()
        {
            backButton.onClick.AddListener(Close);
            foreach (var c in cards)
            {
                var d = c.d;
                c.button.onClick.AddListener(() => Choose(d));
            }
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            openTime = 0f;
            UIAudio.Play(Sfx.UIOpen);
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            UIAudio.Play(Sfx.UIClose);
        }

        // Enter = start with the highlighted (last used) difficulty.
        public void Confirm() => Choose(MetaProgress.SelectedDifficulty);

        private void Choose(Difficulty d)
        {
            MetaProgress.SelectedDifficulty = d;
            IsOpen = false;
            menu.Play();
        }

        private void Update()
        {
            openTime += Time.unscaledDeltaTime;
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = IsOpen;
            panel.localScale = Vector3.one * UIFactory.PopScale(group.alpha);

            // Cards rise in one after another
            for (int i = 0; i < cards.Count; i++)
            {
                float k = Mathf.Clamp01((openTime - 0.05f - i * 0.06f) / 0.3f);
                float e = 1f - (1f - k) * (1f - k);
                var rt = cards[i].rt;
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, Mathf.Lerp(-60f, 0f, IsOpen ? e : 1f) - 20f);
            }
        }

        private void Refresh()
        {
            foreach (var c in cards)
            {
                bool selected = c.d == MetaProgress.SelectedDifficulty;
                var info = DifficultyTable.Get(c.d);
                c.card.color = selected ? Color.Lerp(T.cardSelectedBase, info.Color, 0.18f) : T.cardBackgroundSoft;
            }
        }
    }
}
