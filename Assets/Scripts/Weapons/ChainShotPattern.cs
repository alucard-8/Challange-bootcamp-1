using System;
using System.Collections.Generic;
using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // Chain Lightning: zaps the enemy under the cursor, then arcs to nearby enemies, losing power each jump.
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Chain Lightning", fileName = "ChainShot")]
    public class ChainShotPattern : ShotPattern
    {
        [Min(0)] public int jumps = 4;
        public float firstHitRadius = 1.1f;
        public float jumpRadius = 3.8f;
        [Range(0.1f, 1f)] public float falloff = 0.85f;
        [Min(0.1f)] public float damageMultiplier = 1.5f;

        public static event Action<List<Vector2>, Color> OnChain;   // (points, color) — for the lightning visual

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => OnChain = null;

        public override void Fire(ShotContext ctx)
        {
            var points = new List<Vector2> { ctx.Origin + ctx.Direction * 1.9f };
            var visited = new HashSet<Enemy>();
            float damage = ctx.Damage * damageMultiplier;

            var current = Nearest(ctx.Target, firstHitRadius, visited);
            if (current == null) points.Add(ctx.Target);

            for (int i = 0; current != null && i <= jumps; i++)
            {
                visited.Add(current);
                Vector2 pos = current.transform.position;
                points.Add(pos);
                current.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damage)));
                damage *= falloff;
                current = Nearest(pos, jumpRadius, visited);
            }

            OnChain?.Invoke(points, color);
        }

        private static Enemy Nearest(Vector2 point, float radius, HashSet<Enemy> skip)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics2D.OverlapCircleAll(point, radius))
            {
                if (!hit.TryGetComponent(out Enemy e) || e.Health <= 0 || skip.Contains(e)) continue;
                float d = ((Vector2)e.transform.position - point).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = e; }
            }
            return best;
        }
    }
}
