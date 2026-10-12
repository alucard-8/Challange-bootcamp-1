using UnityEngine;

namespace Soulbreak.Fx
{
    // Tiny one-shot animator: drives one shader float 0 -> 1 (and optionally scale) then self-destructs.
    [RequireComponent(typeof(SpriteRenderer))]
    public class FxAnimator : MonoBehaviour
    {
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;
        private int propertyId;
        private float duration, age;
        private Vector3 scaleFrom, scaleTo;

        public FxAnimator Init(MaterialPropertyBlock block, string property, float duration, Vector3 scaleFrom, Vector3 scaleTo)
        {
            sr = GetComponent<SpriteRenderer>();
            this.block = block;
            propertyId = Shader.PropertyToID(property);
            this.duration = duration;
            this.scaleFrom = scaleFrom;
            this.scaleTo = scaleTo;
            Step(0f);
            return this;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            Step(t);
            if (t >= 1f) Destroy(gameObject);
        }

        private void Step(float t)
        {
            transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, 1f - (1f - t) * (1f - t));
            block.SetFloat(propertyId, t);
            sr.SetPropertyBlock(block);
        }
    }
}
