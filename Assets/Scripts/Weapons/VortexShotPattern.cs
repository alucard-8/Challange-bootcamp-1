using UnityEngine;

namespace Soulbreak.Weapons
{
    // Black Hole: flies to the cursor, pulls enemies in and grinds them, then implodes.
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Black Hole", fileName = "VortexShot")]
    public class VortexShotPattern : ShotPattern
    {
        public float travelSpeed = 11f;
        public float size = 2.6f;
        public float pullRadius = 3.4f;
        public float pullStrength = 5f;
        public float duration = 2.2f;
        public float tickInterval = 0.4f;
        [Min(0.1f)] public float tickDamageMultiplier = 0.5f;
        public float implodeRadius = 2.4f;
        [Min(0.1f)] public float implodeDamageMultiplier = 3f;
        public Sprite sprite;
        public Material material;
        public ExplosionEventChannel explodedChannel;

        public override void Fire(ShotContext ctx) =>
            Vortex.Spawn(this, ctx.Origin + ctx.Direction * 1.9f, ctx.Target, ctx.Damage);
    }
}
