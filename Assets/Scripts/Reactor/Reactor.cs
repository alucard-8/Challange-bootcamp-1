using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Reactor
{
    // Subject in the Observer pattern: owns reactor state and only announces changes.
    // It never references UI, audio or game-over logic — those subscribe to its events.
    public class Reactor : MonoBehaviour, IDamageable
    {
        [SerializeField] private ReactorConfig config;

        public int Health { get; private set; }
        public int ShieldCharges { get; private set; }
        public int MaxHealth => config.maxHealth;
        public int MaxShieldCharges => config.maxShieldCharges;
        public bool IsDestroyed => Health <= 0;

        public event Action<int, int> OnHealthChanged;   // (current, max)
        public event Action<int, int> OnShieldChanged;   // (current, max)
        public event Action OnDestroyed;

        private void Awake()
        {
            Health = config.maxHealth;
            ShieldCharges = config.maxShieldCharges;
        }

        private void Start()
        {
            // Push initial state so listeners (e.g. HUD) can draw themselves.
            OnHealthChanged?.Invoke(Health, MaxHealth);
            OnShieldChanged?.Invoke(ShieldCharges, MaxShieldCharges);
        }

        // Shield charges absorb hits first, then health takes damage.
        public void TakeDamage(int amount)
        {
            if (IsDestroyed || amount <= 0) return;

            if (ShieldCharges > 0)
            {
                ShieldCharges--;
                OnShieldChanged?.Invoke(ShieldCharges, MaxShieldCharges);
                return;
            }

            Health = Mathf.Max(0, Health - amount);
            OnHealthChanged?.Invoke(Health, MaxHealth);

            if (IsDestroyed) OnDestroyed?.Invoke();
        }

        // Called by healing orbs.
        public void RestoreShieldCharge()
        {
            if (IsDestroyed || ShieldCharges >= MaxShieldCharges) return;
            ShieldCharges++;
            OnShieldChanged?.Invoke(ShieldCharges, MaxShieldCharges);
        }
    }
}
