using System;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // A glowing mote that arcs from where souls were earned to the soul counter, then calls back on arrival.
    public class FlyingMote : MonoBehaviour
    {
        private RectTransform rt;
        private Vector2 from, control, to;
        private float duration, age, delay;
        private Action onArrive;
        private Image image;

        public void Launch(Vector2 start, Vector2 end, float delaySeconds, Action arrive)
        {
            rt = (RectTransform)transform;
            image = GetComponent<Image>();
            from = start;
            to = end;
            // Curve outward to the side for a nice swoop
            Vector2 mid = (start + end) * 0.5f;
            Vector2 perp = Vector2.Perpendicular(end - start).normalized;
            control = mid + perp * UnityEngine.Random.Range(-220f, 220f) + Vector2.down * 80f;
            duration = UnityEngine.Random.Range(0.45f, 0.7f);
            delay = delaySeconds;
            onArrive = arrive;
            rt.anchoredPosition = start;
            image.enabled = false;
        }

        private void Update()
        {
            if (delay > 0f) { delay -= Time.unscaledDeltaTime; return; }
            image.enabled = true;

            age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(age / duration);
            float e = t * t * (3f - 2f * t);
            Vector2 a = Vector2.Lerp(from, control, e);
            Vector2 b = Vector2.Lerp(control, to, e);
            rt.anchoredPosition = Vector2.Lerp(a, b, e);
            rt.localScale = Vector3.one * Mathf.Lerp(1.3f, 0.6f, t);
            if (t < 1f) return;

            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
