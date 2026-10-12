using UnityEngine;

namespace Soulbreak.Fx
{
    // A small burst of glowing sparks flying out from an impact point, slowing and fading.
    public class SparkFx : MonoBehaviour
    {
        private Transform[] sparks;
        private Vector2[] velocities;
        private SpriteRenderer[] renderers;
        private float age, life;
        private Color color;

        public static void Burst(Vector2 position, Color color, int count, float speed, Sprite sprite, Material material, float size = 0.18f)
        {
            var go = new GameObject("Sparks");
            go.transform.position = position;
            var fx = go.AddComponent<SparkFx>();
            fx.color = color;
            fx.life = 0.35f;
            fx.sparks = new Transform[count];
            fx.velocities = new Vector2[count];
            fx.renderers = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                var sr = new GameObject("Spark").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(go.transform, false);
                sr.transform.localScale = Vector3.one * size * Random.Range(0.6f, 1.2f);
                sr.sprite = sprite;
                sr.sharedMaterial = material;
                sr.color = color;
                sr.sortingOrder = 8;
                fx.sparks[i] = sr.transform;
                fx.renderers[i] = sr;
                float a = Random.value * Mathf.PI * 2f;
                fx.velocities[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.5f, 1.2f);
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            float k = age / life;
            for (int i = 0; i < sparks.Length; i++)
            {
                velocities[i] *= Mathf.Exp(-6f * dt);
                sparks[i].localPosition += (Vector3)(velocities[i] * dt);
                renderers[i].color = new Color(color.r, color.g, color.b, 1f - k);
            }
            if (k >= 1f) Destroy(gameObject);
        }
    }
}
