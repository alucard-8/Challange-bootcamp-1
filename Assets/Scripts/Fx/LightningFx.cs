using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Fx
{
    // Crackling lightning through a list of points: re-jitters its zig-zag every few frames, then fades.
    [RequireComponent(typeof(LineRenderer))]
    public class LightningFx : MonoBehaviour
    {
        private const int Subdivisions = 6;

        private LineRenderer line;
        private List<Vector2> points;
        private Color color;
        private float age, duration, jitterTimer, width;

        public void Init(List<Vector2> points, Material material, Color color, float width, float duration)
        {
            this.points = new List<Vector2>(points);
            this.color = color;
            this.width = width;
            this.duration = duration;
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = material;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.sortingOrder = 9;
            Rebuild();
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            jitterTimer -= Time.deltaTime;
            if (jitterTimer <= 0f) { jitterTimer = 0.035f; Rebuild(); }

            line.startColor = line.endColor = new Color(color.r, color.g, color.b, 1f - t);
            line.widthMultiplier = width * (1f - t * 0.6f);
            if (t >= 1f) Destroy(gameObject);
        }

        private void Rebuild()
        {
            var pts = new List<Vector3>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 a = points[i], b = points[i + 1];
                Vector2 normal = Vector2.Perpendicular((b - a).normalized);
                float amp = Mathf.Min(0.35f, (b - a).magnitude * 0.12f);
                for (int s = 0; s < Subdivisions; s++)
                {
                    float f = s / (float)Subdivisions;
                    float jitter = s == 0 ? 0f : Random.Range(-amp, amp);
                    pts.Add(Vector2.Lerp(a, b, f) + normal * jitter);
                }
            }
            pts.Add(points[points.Count - 1]);
            line.positionCount = pts.Count;
            line.SetPositions(pts.ToArray());
        }
    }
}
