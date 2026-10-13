using UnityEngine;

namespace Soulbreak.Weapons
{
    [CreateAssetMenu(menuName = "Soulbreak/Shots/Projectile", fileName = "ProjectileShot")]
    public class ProjectileShotPattern : ShotPattern
    {
        [Header("Volley")]
        [Min(1)] public int count = 3;
        [Tooltip("Total spread angle of the volley in degrees")]
        public float spreadAngle = 14f;

        [Header("Projectile")]
        public Sprite sprite;
        public Material material;
        [Min(0.1f)] public float speed = 14f;
        [Min(0.05f)] public float size = 0.35f;
        [Min(0.1f)] public float damageMultiplier = 1f;
        [Min(1f)] public float range = 16f;

        [Header("Behaviour")]
        [Tooltip("Fly to the cursor point and explode there, ignoring anything on the way")]
        public bool travelToTarget;
        [Min(0f)] public float explosionRadius;
        public bool homing;
        [Min(0f)] public float turnSpeed = 540f;

        [Header("Events")]
        public ExplosionEventChannel explodedChannel;

        public override void Fire(ShotContext ctx)
        {
            int damage = Mathf.Max(1, Mathf.RoundToInt(ctx.Damage * damageMultiplier));
            float baseAngle = Mathf.Atan2(ctx.Direction.y, ctx.Direction.x) * Mathf.Rad2Deg;

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (count - 1f);
                float angle = baseAngle + Mathf.Lerp(-spreadAngle, spreadAngle, t) * 0.5f;
                var dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                PlayerProjectile.Spawn(this, ctx.Origin, dir, ctx.Target, damage, ctx.Crit);
            }
        }
    }
}
