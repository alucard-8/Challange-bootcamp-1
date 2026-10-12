using System;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Player
{
    // Holds the aim and fires shots on command. Which shot fires is decided by the weapon ring slot the
    // aim passes through; the shot itself is a ShotPattern strategy. Input lives in PlayerController.
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private WeaponRing weapons;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Camera cam;

        public Vector2 AimPoint { get; private set; }
        public Vector2 AimDirection { get; private set; } = Vector2.up;

        public event Action<Vector2, Vector2, bool, ShotPattern> OnShotFired;   // (from, to, crit, pattern)

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        public void AimAtScreenPoint(Vector2 screenPoint)
        {
            AimPoint = cam.ScreenToWorldPoint(screenPoint);
            Vector2 toAim = AimPoint - (Vector2)muzzle.position;
            if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;
        }

        public bool Fire()
        {
            if (Time.timeScale == 0f || !ammo.TryConsume()) return false;

            Vector2 origin = muzzle.position;
            bool crit = UnityEngine.Random.value < stats.Get(StatType.CritChance);
            int damage = Mathf.Max(1, Mathf.RoundToInt(stats.Get(StatType.Damage) * (crit ? stats.Get(StatType.CritMultiplier) : 1f)));
            int slot = weapons.GetSlotIndex(AimDirection);
            var pattern = weapons.GetPatternAt(slot);

            pattern.Fire(new ShotContext
            {
                Origin = origin,
                Target = AimPoint,
                Direction = AimDirection,
                Damage = damage,
                Crit = crit,
                Stats = stats,
            });
            weapons.Consume(slot);   // module slots empty out and reload a new module
            OnShotFired?.Invoke(origin, AimPoint, crit, pattern);
            return true;
        }
    }
}
