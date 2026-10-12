using UnityEngine;
using UnityEngine.EventSystems;

namespace Soulbreak.UI
{
    // Hover / press juice for buttons and cards: eases scale up on hover, squashes on press.
    // Uses unscaled time so it still animates while the game is paused.
    public class UIButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float hoverScale = 1.06f;
        public float pressScale = 0.95f;
        public float lift = 0f;

        private bool hover, pressed;
        public bool IsHovered => hover;
        private Vector3 baseScale = Vector3.one;
        private Vector2 basePos;
        private RectTransform rt;
        private bool initialized;

        private void Start()
        {
            rt = (RectTransform)transform;
            baseScale = transform.localScale;
            basePos = rt.anchoredPosition;
            initialized = true;
        }

        // Call after moving the rect from code so the lift starts from the new spot.
        public void Rebase()
        {
            if (rt == null) rt = (RectTransform)transform;
            basePos = rt.anchoredPosition;
        }

        public bool sounds = true;

        public void OnPointerEnter(PointerEventData e)
        {
            hover = true;
            if (sounds) Audio.UIAudio.Play(Audio.Sfx.UIHover, 0.35f);
        }

        public void OnPointerExit(PointerEventData e) { hover = false; pressed = false; }

        public void OnPointerDown(PointerEventData e)
        {
            pressed = true;
            if (sounds) Audio.UIAudio.Play(Audio.Sfx.UIClick, 0.6f);
        }
        public void OnPointerUp(PointerEventData e) => pressed = false;

        private void OnDisable() { hover = pressed = false; }

        private void Update()
        {
            if (!initialized) return;
            float target = pressed ? pressScale : hover ? hoverScale : 1f;
            float k = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, k);
            if (lift != 0f)
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, basePos + Vector2.up * (hover ? lift : 0f), k);
        }
    }
}
