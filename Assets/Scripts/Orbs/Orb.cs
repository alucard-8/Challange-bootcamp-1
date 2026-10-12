using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Orbs
{
    public enum OrbType { Soul, Heal }

    // A shootable pickup. Shooting it raises OnAnyCollected; it doesn't know who rewards the player.
    public class Orb : MonoBehaviour, IShootable
    {
        public static event Action<Orb> OnAnyCollected;

        public OrbType Type { get; private set; }
        public int Value { get; private set; }

        private float lifetime;
        private float age;
        private float baseSize;
        private SpriteRenderer sr;
        private Color baseColor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnAnyCollected = null;

        public void Init(OrbType type, int value, float lifetime, float size, Color color)
        {
            Type = type;
            Value = value;
            this.lifetime = lifetime;
            baseSize = size;
            baseColor = color;
            sr = GetComponent<SpriteRenderer>();
            sr.color = color;
        }

        private void Update()
        {
            age += Time.deltaTime;
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Pulse, and blink during the last second so the player knows it's about to vanish.
            transform.localScale = Vector3.one * baseSize * (1f + 0.15f * Mathf.Sin(age * 8f));
            bool blink = lifetime - age < 1f && Mathf.Repeat(age * 10f, 1f) > 0.5f;
            sr.color = blink ? new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f) : baseColor;
        }

        public void Shoot(int damage)
        {
            OnAnyCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
