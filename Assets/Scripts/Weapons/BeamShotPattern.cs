using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Weapons
{
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Beam", fileName = "BeamShot")]
    public class BeamShotPattern : ShotPattern
    {
        public float length = 26f;
        public float width = 0.4f;
        [Min(0.1f)] public float damageMultiplier = 2f;
        public BeamEventChannel beamChannel;

        private static readonly HashSet<IShootable> Hit = new HashSet<IShootable>();

        public override void Fire(ShotContext ctx)
        {
            Vector2 start = ctx.Origin + ctx.Direction * 1.9f;
            int damage = Mathf.Max(1, Mathf.RoundToInt(ctx.Damage * damageMultiplier));

            Hit.Clear();
            foreach (var h in Physics2D.CircleCastAll(start, width, ctx.Direction, length))
                if (h.collider.TryGetComponent(out IShootable s) && Hit.Add(s)) s.Shoot(damage);

            if (beamChannel != null)
                beamChannel.Raise(new BeamInfo { From = start, To = start + ctx.Direction * length, Width = width, Color = color });
        }
    }
}
