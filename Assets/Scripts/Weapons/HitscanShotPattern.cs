using System.Collections.Generic;
using Soulbreak.Core;
using Soulbreak.Core.Events;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // The default click: instantly hits whatever is under the cursor (plus Shrapnel splash).
    // Clicking a target's weak point (enemy eyes) is a HEADSHOT: extra damage + headshot event.
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Hitscan", fileName = "HitscanShot")]
    public class HitscanShotPattern : ShotPattern
    {
        public Vector2EventChannel headshotChannel;

        private static readonly HashSet<IShootable> HitBuffer = new HashSet<IShootable>();

        public override void Fire(ShotContext ctx)
        {
            float splash = ctx.Stats.Get(StatType.SplashRadius);
            var hits = splash > 0f ? Physics2D.OverlapCircleAll(ctx.Target, splash) : Physics2D.OverlapPointAll(ctx.Target);

            HitBuffer.Clear();
            bool first = true;
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out IShootable shootable) || !HitBuffer.Add(shootable)) continue;

                int damage = ctx.Damage;
                if (first && hit.TryGetComponent(out IWeakPoint weak) && weak.IsWeakPointHit(ctx.Target))
                {
                    damage = Mathf.RoundToInt(damage * ctx.Stats.Get(StatType.HeadshotMultiplier));
                    if (headshotChannel != null) headshotChannel.Raise(ctx.Target);
                }
                first = false;

                shootable.Shoot(damage);
                if (splash <= 0f) break; // no splash = one target per click
            }
        }
    }
}
