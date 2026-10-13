using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.MapEvents
{
    public class SoulUrn : MonoBehaviour, IShootable
    {
        public event Action<SoulUrn> OnHit;
        public event Action<SoulUrn> OnBroken;
        public event Action<SoulUrn> OnEscaped;

        public int HitsLeft { get; private set; }

        private Vector2 velocity;
        private float lifetime;
        private SpriteRenderer sr;
        private float wobble;

        public static SoulUrn Spawn(Vector2 position, Vector2 velocity, int hits, float lifetime, Sprite sprite, Material material)
        {
            var go = new GameObject("SoulUrn");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 1.4f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sharedMaterial = material;
            sr.color = new Color(1f, 0.8f, 0.25f);
            sr.sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var urn = go.AddComponent<SoulUrn>();
            urn.sr = sr;
            urn.velocity = velocity;
            urn.HitsLeft = hits;
            urn.lifetime = lifetime;
            return urn;
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            wobble = Mathf.Max(0f, wobble - Time.deltaTime * 6f);
            transform.localScale = Vector3.one * 1.4f * (1f + 0.25f * wobble);

            lifetime -= Time.deltaTime;
            if (lifetime > 0f) return;
            OnEscaped?.Invoke(this);
            Destroy(gameObject);
        }

        public void Shoot(int damage)
        {
            if (HitsLeft <= 0) return;
            HitsLeft--;
            wobble = 1f;
            OnHit?.Invoke(this);
            if (HitsLeft > 0) return;
            OnBroken?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
