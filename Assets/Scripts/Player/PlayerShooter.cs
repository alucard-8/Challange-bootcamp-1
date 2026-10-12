using System;
using Soulbreak.Core;
using Soulbreak.Weapons;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Soulbreak.Player
{
    // Tracks the aim and turns clicks into shots. Which shot fires is decided by the weapon ring
    // slot the aim passes through; the shot itself is a ShotPattern strategy.
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

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            Vector2 origin = muzzle.position;
            AimPoint = cam.ScreenToWorldPoint(InputReader.PointerPosition());
            Vector2 toAim = AimPoint - origin;
            if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;

            if (!InputReader.FirePressed()) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            if (!ammo.TryConsume()) return;

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
        }
    }
}
