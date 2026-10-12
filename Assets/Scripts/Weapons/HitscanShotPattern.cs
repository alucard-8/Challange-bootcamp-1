using System.Collections.Generic;
using Soulbreak.Core;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // The default click: instantly hits whatever is under the cursor (plus Shrapnel splash).
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Hitscan", fileName = "HitscanShot")]
    public class HitscanShotPattern : ShotPattern
    {
        private static readonly HashSet<IShootable> HitBuffer = new HashSet<IShootable>();

        public override void Fire(ShotContext ctx)
        {
            float splash = ctx.Stats.Get(StatType.SplashRadius);
            var hits = splash > 0f ? Physics2D.OverlapCircleAll(ctx.Target, splash) : Physics2D.OverlapPointAll(ctx.Target);

            HitBuffer.Clear();
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out IShootable shootable) || !HitBuffer.Add(shootable)) continue;
                shootable.Shoot(ctx.Damage);
                if (splash <= 0f) break; // no splash = one target per click
            }
        }
    }
}
