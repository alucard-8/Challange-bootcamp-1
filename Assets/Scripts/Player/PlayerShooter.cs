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

        [Header("Aim assist")]
        [SerializeField] private float aimAssistAngle = 22f;
        [SerializeField] private float aimAssistStrength = 0.35f;
        private Enemies.EnemyRegistry enemies;

        public event Action<Vector2, Vector2, bool, ShotPattern> OnShotFired;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        public void AimAtScreenPoint(Vector2 screenPoint)
        {
            AimAtWorldPoint(cam.ScreenToWorldPoint(screenPoint));
            if (Core.GameSettings.AimAssist) ApplyAimAssist();
        }

        public void AimAtWorldPoint(Vector2 worldPoint)
        {
            AimPoint = worldPoint;
            Vector2 toAim = AimPoint - (Vector2)muzzle.position;
            if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;
        }

        private void ApplyAimAssist()
        {
            if (enemies == null) enemies = FindFirstObjectByType<Enemies.EnemyRegistry>();
            if (enemies == null) return;

            Vector2 origin = muzzle.position;
            Vector2 best = Vector2.zero;
            float bestDot = Mathf.Cos(aimAssistAngle * Mathf.Deg2Rad);
            bool found = false;
            foreach (var e in enemies.Alive)
            {
                if (e == null || !e.IsAlive) continue;
                Vector2 to = (Vector2)e.transform.position - origin;
                if (to.sqrMagnitude < 0.0001f) continue;
                float dot = Vector2.Dot(AimDirection, to.normalized);
                if (dot > bestDot) { bestDot = dot; best = to; found = true; }
            }

            if (!found) return;
            AimDirection = Vector2.Lerp(AimDirection, best.normalized, Mathf.Clamp01(aimAssistStrength)).normalized;
            AimPoint = origin + AimDirection * best.magnitude;
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
