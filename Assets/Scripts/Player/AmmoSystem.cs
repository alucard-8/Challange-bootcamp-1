using System;
using UnityEngine;

namespace Soulbreak.Player
{
    // Owns the clip. The shooter asks it for permission to fire; it never knows what is shot or reads input.
    // Ammo recharges itself: whenever the clip is below max it regains one shot every ReloadTime seconds, so
    // the player never reloads by hand. Ammo charges still top the clip off instantly.
    public class AmmoSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Orbs.OrbEventChannel orbCollected;

        public int Current { get; private set; }
        public int Max => Mathf.Max(1, stats.GetInt(StatType.ClipSize));
        public bool IsReloading => Current < Max;                                  // recharging toward a full clip
        public float PerShot => Mathf.Max(0.02f, stats.Get(StatType.ReloadTime));  // seconds to regain one shot
        public float ReloadProgress => IsReloading ? Mathf.Clamp01(1f - rechargeTimer / PerShot) : 1f;

        public event Action<int, int> OnAmmoChanged;   // (current, max)
        public event Action<float> OnReloadStarted;    // fired the moment the clip runs dry (seconds per shot)
        public event Action OnReloadFinished;          // fired when the clip is full again
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

        // Ammo charge picked up: fill the clip instantly.
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
                rechargeTimer = PerShot;   // a full clip waits a whole interval before the next shot returns
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
