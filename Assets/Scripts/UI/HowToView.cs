using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // "How to play" popup, shared by the main menu and the pause menu. Pure display.
    // Copy lives in UIStrings and is applied on Awake so it can be edited without touching the prefab.
    public class HowToView : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button backButton;
        [SerializeField] private Text titleText, bodyLeft, bodyRight, controlsText, backLabel;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private void Awake()
        {
            backButton.onClick.AddListener(Close);
            ApplyText();
        }

        private void ApplyText()
        {
            if (titleText != null) titleText.text = S.howToTitle;
            if (bodyLeft != null) bodyLeft.text = S.howToBodyLeft;
            if (bodyRight != null) bodyRight.text = S.howToBodyRight;
            if (controlsText != null) controlsText.text = S.howToControls;
            if (backLabel != null) backLabel.text = S.howToBack;
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Audio.UIAudio.Play(Audio.Sfx.UIClose);
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = IsOpen;
            panel.localScale = Vector3.one * UIFactory.PopScale(group.alpha);
        }
    }
}
