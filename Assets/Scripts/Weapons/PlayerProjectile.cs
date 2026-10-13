using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Weapons
{
    public class PlayerProjectile : MonoBehaviour
    {
        private ProjectileShotPattern pattern;
        private Vector2 velocityDir, targetPoint;
        private int damage;
        private float travelled, hitRadius;
        private Transform homingTarget;
        private float retargetTimer;

        public static void Spawn(ProjectileShotPattern pattern, Vector2 origin, Vector2 dir, Vector2 target, int damage, bool crit)
        {
            var go = new GameObject(pattern.displayName);
            go.transform.position = origin + dir * 1.9f;
            go.transform.localScale = Vector3.one * pattern.size * (crit ? 1.25f : 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = pattern.sprite;
            if (pattern.material != null) sr.sharedMaterial = pattern.material;
            sr.color = pattern.color;
            sr.sortingOrder = 6;

            var p = go.AddComponent<PlayerProjectile>();
            p.pattern = pattern;
            p.velocityDir = dir;
            p.targetPoint = target;
            p.damage = damage;
            p.hitRadius = pattern.size * 0.45f;
            if (pattern.homing) p.homingTarget = NearestHostile(target, 6f);
        }

        private void Update()
        {
            if (pattern.homing) Steer();

            float step = pattern.speed * Time.deltaTime;
            Vector2 pos = transform.position;

            if (pattern.travelToTarget)
            {
                float remaining = Vector2.Distance(pos, targetPoint);
                if (remaining <= step) { transform.position = targetPoint; Explode(); return; }
                velocityDir = (targetPoint - pos).normalized;
            }

            transform.position = pos + velocityDir * step;
            travelled += step;

            if (!pattern.travelToTarget && TryHit()) return;
            if (travelled >= pattern.range) Destroy(gameObject);
        }

        private void Steer()
        {
            retargetTimer -= Time.deltaTime;
            if (homingTarget == null && retargetTimer <= 0f)
            {
                homingTarget = NearestHostile(transform.position, 12f);
                retargetTimer = 0.2f;
            }
            if (homingTarget == null) return;

            Vector2 desired = ((Vector2)homingTarget.position - (Vector2)transform.position).normalized;
            float maxRad = pattern.turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            velocityDir = Vector3.RotateTowards(velocityDir, desired, maxRad, 0f);
        }

        private bool TryHit()
        {
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, hitRadius))
            {
                if (!hit.TryGetComponent(out IHostile hostile) || !hostile.IsAlive) continue;

                if (pattern.explosionRadius > 0f) Explode();
                else { hostile.Shoot(damage); Destroy(gameObject); }
                return true;
            }
            return false;
        }

        private void Explode()
        {
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, pattern.explosionRadius))
                if (hit.TryGetComponent(out IShootable shootable)) shootable.Shoot(damage);

            if (pattern.explodedChannel != null)
                pattern.explodedChannel.Raise(new ExplosionInfo { Position = transform.position, Radius = pattern.explosionRadius, Color = pattern.color });
            Destroy(gameObject);
        }

        public static Transform NearestHostile(Vector2 point, float radius)
        {
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics2D.OverlapCircleAll(point, radius))
            {
                if (!hit.TryGetComponent(out IHostile h) || !h.IsAlive) continue;
                float d = ((Vector2)h.transform.position - point).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = h.transform; }
            }
            return best;
        }
    }
}
