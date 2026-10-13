using System;
using UnityEngine;

namespace Soulbreak.Player
{
    public class AmmoSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Orbs.OrbEventChannel orbCollected;

        public int Current { get; private set; }
        public int Max => Mathf.Max(1, stats.GetInt(StatType.ClipSize));
        public bool IsReloading => Current < Max;
        public float PerShot => Mathf.Max(0.02f, stats.Get(StatType.ReloadTime));
        public float ReloadProgress => IsReloading ? Mathf.Clamp01(1f - rechargeTimer / PerShot) : 1f;

        public event Action<int, int> OnAmmoChanged;
        public event Action<float> OnReloadStarted;
        public event Action OnReloadFinished;
        public event Action OnDryFire;
        public event Action OnAmmoPickup;

        private float rechargeTimer;

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

        private void HandleOrb(Orbs.Orb orb)
        {
            if (orb.Type != Orbs.OrbType.Ammo) return;
            bool wasFull = Current >= Max;
            Refill();
            if (!wasFull) OnReloadFinished?.Invoke();
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

            if (Current >= Max)
            {
                rechargeTimer = PerShot;
                return;
            }

            rechargeTimer -= Time.deltaTime;
            if (rechargeTimer > 0f) return;

            Current++;
            OnAmmoChanged?.Invoke(Current, Max);
            rechargeTimer = PerShot;
            if (Current >= Max) OnReloadFinished?.Invoke();
        }

        public bool TryConsume()
        {
            if (Current <= 0)
            {
                OnDryFire?.Invoke();
                return false;
            }

            Current--;
            OnAmmoChanged?.Invoke(Current, Max);
            if (Current == 0) OnReloadStarted?.Invoke(PerShot);
            return true;
        }

        public void Refill()
        {
            Current = Max;
            OnAmmoChanged?.Invoke(Current, Max);
        }
    }
}
