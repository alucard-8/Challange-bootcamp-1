using System;
using UnityEngine;

namespace Soulbreak.Fx
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FxAnimator : MonoBehaviour
    {
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;
        private int propertyId;
        private float duration, age;
        private Vector3 scaleFrom, scaleTo;
        private Action onComplete;

        public FxAnimator Init(MaterialPropertyBlock block, string property, float duration, Vector3 scaleFrom, Vector3 scaleTo, Action onComplete = null)
        {
            sr = GetComponent<SpriteRenderer>();
            this.block = block;
            propertyId = Shader.PropertyToID(property);
            this.duration = duration;
            this.scaleFrom = scaleFrom;
            this.scaleTo = scaleTo;
            this.onComplete = onComplete;
            age = 0f;
            Step(0f);
            return this;
        }

        private void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(age / duration);
            Step(t);
            if (t < 1f) return;

            if (onComplete != null) onComplete();
            else Destroy(gameObject);
        }

        private void Step(float t)
        {
            transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, 1f - (1f - t) * (1f - t));
            block.SetFloat(propertyId, t);
            sr.SetPropertyBlock(block);
        }
    }
}
