using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    [RequireComponent(typeof(Text))]
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float duration = 0.9f;
        [SerializeField] private float rise = 70f;

        private Text text;
        private RectTransform rt;
        private Vector2 start;
        private float age;

        private void Awake()
        {
            text = GetComponent<Text>();
            rt = (RectTransform)transform;
        }

        private void Start() => start = rt.anchoredPosition;

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = age / duration;
            rt.anchoredPosition = start + Vector2.up * rise * (1f - (1f - t) * (1f - t));
            var c = text.color;
            c.a = 1f - t * t;
            text.color = c;
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
