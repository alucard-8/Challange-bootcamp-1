using UnityEngine;

namespace Soulbreak.Weapons
{
    // Orbital Blades: summons blades that circle the reactor for a while, slicing whatever they touch.
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Orbital Blades", fileName = "OrbitShot")]
    public class OrbitShotPattern : ShotPattern
    {
        [Min(1)] public int blades = 3;
        public float radius = 2.8f;
        public float angularSpeed = 260f;
        public float duration = 6f;
        public float hitCooldown = 0.35f;
        public float bladeSize = 0.8f;
        [Min(0.1f)] public float damageMultiplier = 1f;
        public Sprite sprite;
        public Material material;

        public override void Fire(ShotContext ctx)
        {
            int damage = Mathf.Max(1, Mathf.RoundToInt(ctx.Damage * damageMultiplier));
            float startAngle = Mathf.Atan2(ctx.Direction.y, ctx.Direction.x) * Mathf.Rad2Deg;
            OrbitBlades.Spawn(this, ctx.Origin, startAngle, damage);
        }
    }
}
