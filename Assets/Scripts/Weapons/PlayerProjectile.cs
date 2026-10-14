using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Weapons
{
    public class PlayerProjectile : MonoBehaviour
    {
        private static readonly Stack<PlayerProjectile> pool = new Stack<PlayerProjectile>();

        private ProjectileShotPattern pattern;
        private SpriteRenderer sr;
        private Vector2 velocityDir, targetPoint;
        private int damage;
        private float travelled, hitRadius;
        private Transform homingTarget;
        private float retargetTimer;

        public static void Spawn(ProjectileShotPattern pattern, Vector2 origin, Vector2 dir, Vector2 target, int damage, bool crit)
        {
            PlayerProjectile p = null;
            while (pool.Count > 0)
            {
                var candidate = pool.Pop();
                if (candidate != null) { p = candidate; break; }
            }

            if (p == null)
            {
                var go = new GameObject(pattern.displayName);
                p = go.AddComponent<PlayerProjectile>();
                p.sr = go.AddComponent<SpriteRenderer>();
                p.sr.sortingOrder = 6;
            }
            else
            {
                p.gameObject.SetActive(true);
            }

            p.transform.position = origin + dir * 1.9f;
            p.transform.localScale = Vector3.one * pattern.size * (crit ? 1.25f : 1f);
            p.sr.sprite = pattern.sprite;
            p.sr.sharedMaterial = pattern.material;
            p.sr.color = pattern.color;

            p.pattern = pattern;
            p.velocityDir = dir;
            p.targetPoint = target;
            p.damage = damage;
            p.travelled = 0f;
            p.hitRadius = pattern.size * 0.45f;
            p.homingTarget = null;
            p.retargetTimer = 0f;
            if (pattern.homing) p.homingTarget = NearestHostile(target, 6f);
        }

        private void Despawn()
        {
            gameObject.SetActive(false);
            pool.Push(this);
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
            if (travelled >= pattern.range) Despawn();
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
                else { hostile.Shoot(damage); Despawn(); }
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
            Despawn();
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
