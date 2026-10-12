using System;
using Soulbreak.Core;
using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // A player projectile: flies straight, homes, or travels to a point and explodes.
    // Uses overlap checks instead of physics callbacks, so it needs no Rigidbody.
    public class PlayerProjectile : MonoBehaviour
    {
        public static event Action<Vector2, float, Color> OnAnyExploded;   // (position, radius, color)

        private ProjectileShotPattern pattern;
        private Vector2 velocityDir, targetPoint;
        private int damage;
        private float travelled, hitRadius;
        private Transform homingTarget;
        private float retargetTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnAnyExploded = null;

        // Lets other weapon objects (e.g. the black hole) reuse the explosion event/visual.
        public static void RaiseExplosion(Vector2 pos, float radius, Color color) => OnAnyExploded?.Invoke(pos, radius, color);

        public static void Spawn(ProjectileShotPattern pattern, Vector2 origin, Vector2 dir, Vector2 target, int damage, bool crit)
        {
            var go = new GameObject(pattern.displayName);
            go.transform.position = origin + dir * 1.9f;   // start at the weapon ring
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
            if (pattern.homing) p.homingTarget = NearestEnemy(target, 6f);
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
                homingTarget = NearestEnemy(transform.position, 12f);
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
                bool isTarget = hit.GetComponent<Enemy>() != null || hit.GetComponent<EnemyProjectile>() != null;
                if (!isTarget || !hit.TryGetComponent(out IShootable shootable)) continue;

                if (pattern.explosionRadius > 0f) Explode();
                else { shootable.Shoot(damage); Destroy(gameObject); }
                return true;
            }
            return false;
        }

        private void Explode()
        {
            // Explosions also collect orbs — everything shootable in the blast is hit.
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, pattern.explosionRadius))
                if (hit.TryGetComponent(out IShootable shootable)) shootable.Shoot(damage);

            OnAnyExploded?.Invoke(transform.position, pattern.explosionRadius, pattern.color);
            Destroy(gameObject);
        }

        private static Transform NearestEnemy(Vector2 point, float radius)
        {
            Transform best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics2D.OverlapCircleAll(point, radius))
            {
                if (!hit.TryGetComponent(out Enemy enemy)) continue;
                float d = ((Vector2)enemy.transform.position - point).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = enemy.transform; }
            }
            return best;
        }
    }
}
