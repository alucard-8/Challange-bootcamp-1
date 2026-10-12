using System;

using UnityEngine;

namespace Soulbreak.Player
{
    // Owns the clip. The shooter asks it for permission to fire; it never knows what is shot or reads input.
    public class AmmoSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Orbs.OrbEventChannel orbCollected;

        public int Current { get; private set; }
        public int Max => Mathf.Max(1, stats.GetInt(StatType.ClipSize));
        public bool IsReloading { get; private set; }
        public float ReloadProgress => IsReloading ? 1f - reloadTimer / reloadDuration : 1f;

        public event Action<int, int> OnAmmoChanged;   // (current, max)
        public event Action<float> OnReloadStarted;    // duration
        public event Action OnReloadFinished;
        public event Action OnDryFire;
        public event Action OnAmmoPickup;

        private float reloadTimer, reloadDuration;

        private void OnEnable()
        {
            stats.OnStatChanged += HandleStatChanged;
            if (orbCollected != null) orbCollected.OnRaised += HandleOrb;
        }

        private void OnDisable()
        {
            stats.OnStatChanged -= HandleStatChanged;
            if (orbCollected != null) orbCollected.OnRaised -= HandleOrb;
        }

        // Ammo charge picked up: skip the reload and fill the clip instantly.
        private void HandleOrb(Orbs.Orb orb)
        {
            if (orb.Type != Orbs.OrbType.Ammo) return;
            bool wasReloading = IsReloading;
            IsReloading = false;
            Refill();
            if (wasReloading) OnReloadFinished?.Invoke();
            OnAmmoPickup?.Invoke();
        }

        private void Start() => Refill();

        private void HandleStatChanged(StatType stat, float value)
        {
            if (stat == StatType.ClipSize) Refill();
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            if (IsReloading)
            {
                reloadTimer -= Time.deltaTime;
                if (reloadTimer <= 0f)
                {
                    IsReloading = false;
                    Refill();
                    OnReloadFinished?.Invoke();
                }
            }
        }

        // Manual reload command (sent by PlayerController).
        public void Reload()
        {
            if (!IsReloading && Current < Max) StartReload();
        }

        public bool TryConsume()
        {
            if (IsReloading || Current <= 0)
            {
                OnDryFire?.Invoke();
                if (!IsReloading) StartReload();
                return false;
            }

            Current--;
            OnAmmoChanged?.Invoke(Current, Max);
            if (Current == 0) StartReload();
            return true;
        }

        public void Refill()
        {
            Current = Max;
            OnAmmoChanged?.Invoke(Current, Max);
        }

        private void StartReload()
        {
            IsReloading = true;
            reloadDuration = reloadTimer = stats.Get(StatType.ReloadTime);
            OnReloadStarted?.Invoke(reloadDuration);
        }
    }
}
