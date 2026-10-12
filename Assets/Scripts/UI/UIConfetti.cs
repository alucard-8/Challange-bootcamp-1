using UnityEngine;

namespace Soulbreak.UI
{
    // A burst of glowing UI particles (gold sparks + tiny streamers) that fly out, fall and fade.
    // Real-time, so it works on frozen screens like game over.
    // Lives in the prefab as an inactive template with one inactive "Spark" child; Burst clones both.
    public class UIConfetti : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image spark;

        private RectTransform[] parts;
        private Vector2[] velocity;
        private float[] spin;
        private UnityEngine.UI.Image[] images;
        private float age, life = 1.3f;
        private Color color;

        public static void Burst(UIConfetti template, Vector2 position, Color color, int count = 36, float speed = 650f)
        {
            var c = UIFactory.Spawn(template, template.transform.parent);
            c.color = color;
            c.parts = new RectTransform[count];
            c.velocity = new Vector2[count];
            c.spin = new float[count];
            c.images = new UnityEngine.UI.Image[count];

            for (int i = 0; i < count; i++)
            {
                bool streamer = i % 3 == 0;
                var img = UIFactory.Spawn(c.spark, c.transform);
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = streamer ? new Vector2(5f, 16f) : Vector2.one * Random.Range(6f, 12f);
                rt.anchoredPosition = position;
                rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                float a = Random.Range(0f, Mathf.PI * 2f);
                c.velocity[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.35f, 1f) + Vector2.up * 180f;
                c.spin[i] = Random.Range(-540f, 540f);
                c.parts[i] = rt;
                c.images[i] = img;
            }
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            age += dt;
            float k = age / life;
            for (int i = 0; i < parts.Length; i++)
            {
                velocity[i] *= Mathf.Exp(-2.2f * dt);
                velocity[i].y -= 900f * dt;
                parts[i].anchoredPosition += velocity[i] * dt;
                parts[i].Rotate(0f, 0f, spin[i] * dt);
                float twinkle = 0.75f + 0.25f * Mathf.Sin(age * 30f + i);
                images[i].color = new Color(color.r * twinkle + 0.2f, color.g * twinkle + 0.2f, color.b * twinkle, 1f - k * k);
            }
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
