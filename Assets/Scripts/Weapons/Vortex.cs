using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Weapons
{
    public class Vortex : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int PowerId = Shader.PropertyToID("_Power");

        private VortexShotPattern pattern;
        private Vector2 target;
        private int baseDamage;
        private bool active;
        private float age, tick;
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;

        public static void Spawn(VortexShotPattern pattern, Vector2 start, Vector2 target, int damage)
        {
            var go = new GameObject("BlackHole");
            go.transform.position = start;
            go.transform.localScale = Vector3.one * pattern.size * 0.35f;
            var v = go.AddComponent<Vortex>();
            v.pattern = pattern;
            v.target = target;
            v.baseDamage = damage;
            v.sr = go.AddComponent<SpriteRenderer>();
            v.sr.sprite = pattern.sprite;
            if (pattern.material != null) v.sr.sharedMaterial = pattern.material;
            v.sr.sortingOrder = 6;
            v.block = new MaterialPropertyBlock();
            v.block.SetColor(ColorId, pattern.color);
            v.block.SetFloat(PowerId, 0.3f);
            v.sr.SetPropertyBlock(v.block);
        }

        private void Update()
        {
            if (!active)
            {
                transform.position = Vector2.MoveTowards(transform.position, target, pattern.travelSpeed * Time.deltaTime);
                if ((Vector2)transform.position == target) active = true;
                return;
            }

            age += Time.deltaTime;
            float k = Mathf.Clamp01(age / pattern.duration);
            transform.localScale = Vector3.one * pattern.size * Mathf.Lerp(0.6f, 1f, Mathf.Sqrt(k));
            transform.Rotate(0f, 0f, -200f * Time.deltaTime);
            block.SetFloat(PowerId, Mathf.Lerp(0.5f, 1f, k));
            sr.SetPropertyBlock(block);

            Vector2 c = transform.position;
            tick -= Time.deltaTime;
            bool doTick = tick <= 0f;
            if (doTick) tick = pattern.tickInterval;

            foreach (var hit in Physics2D.OverlapCircleAll(c, pattern.pullRadius))
            {
                if (!hit.TryGetComponent(out IHostile h) || !h.IsAlive) continue;
                if (!h.IsHeavy)
                {
                    float dist = Vector2.Distance(h.transform.position, c);
                    float strength = pattern.pullStrength * (1.2f - dist / pattern.pullRadius);
                    h.transform.position = Vector2.MoveTowards(h.transform.position, c, strength * Time.deltaTime);
                }
                if (doTick) h.Shoot(Mathf.Max(1, Mathf.RoundToInt(baseDamage * pattern.tickDamageMultiplier)));
            }

            if (age >= pattern.duration) Implode(c);
        }

        private void Implode(Vector2 c)
        {
            int dmg = Mathf.Max(1, Mathf.RoundToInt(baseDamage * pattern.implodeDamageMultiplier));
            foreach (var hit in Physics2D.OverlapCircleAll(c, pattern.implodeRadius))
                if (hit.TryGetComponent(out IShootable s)) s.Shoot(dmg);

            if (pattern.explodedChannel != null)
                pattern.explodedChannel.Raise(new ExplosionInfo { Position = c, Radius = pattern.implodeRadius, Color = pattern.color });
            Destroy(gameObject);
        }
    }
}
