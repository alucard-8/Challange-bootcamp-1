using System;
using UnityEngine;

namespace Soulbreak.Fx
{
    [RequireComponent(typeof(LineRenderer))]
    public class TracerFx : MonoBehaviour
    {
        private LineRenderer line;
        private float duration, age, width;
        private Color color;
        private Action onComplete;

        public void Init(Vector2 from, Vector2 to, Material material, Color color, float width, float duration, Action onComplete = null)
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.sharedMaterial = material;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 4;
            line.sortingOrder = 8;
            this.color = color;
            this.width = width;
            this.duration = duration;
            this.onComplete = onComplete;
            age = 0f;
            Apply(0f);
        }

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(age / duration);
            Apply(t);
            if (t < 1f) return;

            if (onComplete != null) onComplete();
            else Destroy(gameObject);
        }

        private void Apply(float t)
        {
            var c = new Color(color.r, color.g, color.b, 1f - t);
            line.startColor = line.endColor = c;
            line.widthMultiplier = width * (1f - t * 0.7f);
        }
    }
}
