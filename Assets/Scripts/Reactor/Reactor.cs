using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Reactor
{
    // Subject in the Observer pattern: owns reactor state and only announces changes.
    // It never references UI, audio, effects or game-over logic — those subscribe to its events.
    public class Reactor : MonoBehaviour, IDamageable
    {
        [SerializeField] private ReactorConfig config;

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int ShieldCharges { get; private set; }
        public int MaxShieldCharges { get; private set; }
        public bool IsDestroyed => Health <= 0;

        public event Action<int, int> OnHealthChanged;   // (current, max)
        public event Action<int, int> OnShieldChanged;   // (current, max)
        public event Action<bool> OnHit;                 // true = shield absorbed it
        public event Action OnDestroyed;

        private void Awake()
        {
            MaxHealth = Health = config.maxHealth;
            MaxShieldCharges = ShieldCharges = config.maxShieldCharges;
        }

        private void Start()
        {
            // Push initial state so listeners (HUD, view) can draw themselves.
            RaiseHealth();
            RaiseShield();
        }

        // Shield charges absorb hits first, then health takes damage.
        public void TakeDamage(int amount)
        {
            if (IsDestroyed || amount <= 0) return;

            if (ShieldCharges > 0)
            {
                ShieldCharges--;
                RaiseShield();
                OnHit?.Invoke(true);
                return;
            }

            Health = Mathf.Max(0, Health - amount);
            RaiseHealth();
            OnHit?.Invoke(false);

            if (IsDestroyed) OnDestroyed?.Invoke();
        }

        public void RestoreShieldCharge(int amount = 1)
        {
            if (IsDestroyed) return;
            ShieldCharges = Mathf.Min(MaxShieldCharges, ShieldCharges + amount);
            RaiseShield();
        }

        public void AddMaxShield(int amount)
        {
            MaxShieldCharges += amount;
            ShieldCharges += amount;
            RaiseShield();
        }

        public void Heal(int amount)
        {
            if (IsDestroyed) return;
            Health = Mathf.Min(MaxHealth, Health + amount);
            RaiseHealth();
        }

        public void AddMaxHealth(int amount)
        {
            MaxHealth += amount;
            Health += amount;
            RaiseHealth();
        }

        private void RaiseHealth() => OnHealthChanged?.Invoke(Health, MaxHealth);
        private void RaiseShield() => OnShieldChanged?.Invoke(ShieldCharges, MaxShieldCharges);
    }
}
