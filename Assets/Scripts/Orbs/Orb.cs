using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Orbs
{
    public enum OrbType { Soul, Heal }

    // A shootable pickup. Shooting it raises OnAnyCollected; it doesn't know who rewards the player.
    // The glow/pulse is done by the Orb shader; this script only handles lifetime and blinking.
    public class Orb : MonoBehaviour, IShootable
    {
        public static event Action<Orb> OnAnyCollected;

        public OrbType Type { get; private set; }
        public int Value { get; private set; }
        public Color Color { get; private set; }

        private float lifetime;
        private float age;
        private SpriteRenderer sr;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnAnyCollected = null;

        public void Init(OrbType type, int value, float lifetime, Color color)
        {
            Type = type;
            Value = value;
            Color = color;
            this.lifetime = lifetime;
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

            // Blink during the last second so the player knows it's about to vanish.
            bool blink = lifetime - age < 1f && Mathf.Repeat(age * 10f, 1f) > 0.5f;
            sr.color = new Color(Color.r, Color.g, Color.b, blink ? 0.25f : 1f);
        }

        public void Shoot(int damage)
        {
            OnAnyCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
