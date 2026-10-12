
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Orbs
{
    public enum OrbType { Soul, Heal, Ammo }

    // A shootable pickup. Shooting it raises its "collected" channel; it doesn't know who rewards the player.
    // The glow/pulse is done by the Orb shader; this script only handles lifetime and blinking.
    public class Orb : MonoBehaviour, IShootable
    {
        private OrbEventChannel collectedChannel;

        public OrbType Type { get; private set; }
        public int Value { get; private set; }
        public Color Color { get; private set; }

        private float lifetime;
        private float age;
        private SpriteRenderer sr;
        private Transform magnet;      // ammo orbs fly to the core on their own
        private Vector2 velocity;

        public void Magnetize(Transform target, Vector2 kick) { magnet = target; velocity = kick; }

        public void Init(OrbType type, int value, float lifetime, Color color, OrbEventChannel collected)
        {
            collectedChannel = collected;
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
            if (magnet != null) { FlyToMagnet(); return; }
            if (age >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Blink during the last second so the player knows it's about to vanish.
            bool blink = lifetime - age < 1f && Mathf.Repeat(age * 10f, 1f) > 0.5f;
            sr.color = new Color(Color.r, Color.g, Color.b, blink ? 0.25f : 1f);
        }

        private void FlyToMagnet()
        {
            // Pop out, hang for a moment, then home in faster and faster.
            Vector2 pos = transform.position, to = (Vector2)magnet.position - pos;
            if (age > 0.45f) velocity += to.normalized * (30f + age * 40f) * Time.deltaTime;
            velocity *= Mathf.Exp(-3f * Time.deltaTime);
            transform.position = pos + velocity * Time.deltaTime;
            transform.Rotate(0f, 0f, 360f * Time.deltaTime);
            if (to.magnitude < 0.7f && age > 0.45f) Shoot(0);
        }

        public void Shoot(int damage)
        {
            if (collectedChannel != null) collectedChannel.Raise(this);
            Destroy(gameObject);
        }
    }
}
