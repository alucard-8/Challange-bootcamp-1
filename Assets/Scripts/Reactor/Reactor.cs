using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Reactor
{
    public class Reactor : MonoBehaviour, IDamageable
    {
        [SerializeField] private ReactorConfig config;

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int ShieldCharges { get; private set; }
        public int MaxShieldCharges { get; private set; }
        public bool IsDestroyed => Health <= 0;
        public bool ShieldSuppressed { get; private set; }

        public void SetShieldSuppressed(bool suppressed)
        {
            if (ShieldSuppressed == suppressed) return;
            ShieldSuppressed = suppressed;
            RaiseShield();
        }

        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnShieldChanged;
        public event Action<bool> OnHit;
        public event Action OnDestroyed;

        private float healthMultiplier = 1f;

        public void SetHealthMultiplier(float multiplier) => healthMultiplier = multiplier;

        private void Awake()
        {
            MaxHealth = Health = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * healthMultiplier));
            MaxShieldCharges = ShieldCharges = config.maxShieldCharges;
        }

        private void Start()
        {
            RaiseHealth();
            RaiseShield();
        }

        public void TakeDamage(int amount)
        {
            if (IsDestroyed || amount <= 0) return;

            if (ShieldCharges > 0 && !ShieldSuppressed)
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
