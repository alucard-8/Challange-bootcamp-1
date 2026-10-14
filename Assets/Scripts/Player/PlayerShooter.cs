using System;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private WeaponRing weapons;
        [SerializeField] private OverdriveSystem overdrive;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Camera cam;

        public Vector2 AimPoint { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.up;

        public Vector2 MuzzlePosition => muzzle.position;

        public bool IgnoreAmmo { get; set; }

        public event Action<Vector2, Vector2, bool, ShotPattern> OnShotFired;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        public void AimAtScreenPoint(Vector2 screenPoint)
        {
            AimAtWorldPoint(cam.ScreenToWorldPoint(screenPoint));
        }

        public void AimAtWorldPoint(Vector2 worldPoint)
        {
            AimPoint = worldPoint;
            Vector2 toAim = AimPoint - (Vector2)muzzle.position;
            if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;
        }

        public bool Fire()
        {
            if (Time.timeScale == 0f) return false;
            bool overdriven = overdrive != null && overdrive.IsActive;
            if (!overdriven && !IgnoreAmmo && !ammo.TryConsume()) return false;

            Vector2 origin = muzzle.position;
            bool crit = UnityEngine.Random.value < stats.Get(StatType.CritChance);
            int damage = Mathf.Max(1, Mathf.RoundToInt(stats.Get(StatType.Damage) * (crit ? stats.Get(StatType.CritMultiplier) : 1f)));
            var ctx = new ShotContext
            {
                Origin = origin,
                Target = AimPoint,
                Direction = AimDirection,
                Damage = damage,
                Crit = crit,
                Stats = stats,
            };

            int slot = weapons.GetSlotIndex(AimDirection);
            var pattern = weapons.GetPatternAt(slot);
            pattern.Fire(ctx);
            OnShotFired?.Invoke(origin, AimPoint, crit, pattern);

            if (overdriven)
            {
                foreach (var module in weapons.OwnedModules)
                    if (module != pattern) { module.Fire(ctx); OnShotFired?.Invoke(origin, AimPoint, crit, module); }
            }
            else weapons.Consume(slot);

            return true;
        }
    }
}
