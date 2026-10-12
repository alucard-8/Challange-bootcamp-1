using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // "How to play" popup, shared by the main menu and the pause menu. Pure display.
    public class HowToView : MonoBehaviour
    {
        public bool IsOpen { get; private set; }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button backButton;

        private void Awake() => backButton.onClick.AddListener(Close);

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
