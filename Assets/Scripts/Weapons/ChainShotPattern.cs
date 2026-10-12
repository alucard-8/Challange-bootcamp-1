using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // Chain Lightning: zaps the hostile under the cursor, then arcs to nearby hostiles, losing power each jump.
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Chain Lightning", fileName = "ChainShot")]
    public class ChainShotPattern : ShotPattern
    {
        [Min(0)] public int jumps = 4;
        public float firstHitRadius = 1.1f;
        public float jumpRadius = 3.8f;
        [Range(0.1f, 1f)] public float falloff = 0.85f;
        [Min(0.1f)] public float damageMultiplier = 1.5f;
        public ChainEventChannel chainChannel;

        public override void Fire(ShotContext ctx)
        {
            var points = new List<Vector2> { ctx.Origin + ctx.Direction * 1.9f };
            var visited = new HashSet<IHostile>();
            float damage = ctx.Damage * damageMultiplier;

            var current = Nearest(ctx.Target, firstHitRadius, visited);
            if (current == null) points.Add(ctx.Target);

            for (int i = 0; current != null && i <= jumps; i++)
            {
                visited.Add(current);
                Vector2 pos = current.transform.position;
                points.Add(pos);
                current.Shoot(Mathf.Max(1, Mathf.RoundToInt(damage)));
                damage *= falloff;
                current = Nearest(pos, jumpRadius, visited);
            }

            if (chainChannel != null) chainChannel.Raise(new ChainInfo { Points = points, Color = color });
        }

        private static IHostile Nearest(Vector2 point, float radius, HashSet<IHostile> skip)
        {
            IHostile best = null;
            float bestDist = float.MaxValue;
            foreach (var hit in Physics2D.OverlapCircleAll(point, radius))
            {
                if (!hit.TryGetComponent(out IHostile h) || !h.IsAlive || skip.Contains(h)) continue;
                float d = ((Vector2)h.transform.position - point).sqrMagnitude;
                if (d < bestDist) { bestDist = d; best = h; }
            }
            return best;
        }
    }
}
