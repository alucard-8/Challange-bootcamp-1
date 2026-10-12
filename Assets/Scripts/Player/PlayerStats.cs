using System;
using UnityEngine;

namespace Soulbreak.Player
{
    // Upgradeable numbers. Perks write here; shooter and wallet read from here.
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] private int baseDamage = 1;
        [SerializeField] private float baseSoulMultiplier = 1f;

        public int Damage { get; private set; }
        public float SoulMultiplier { get; private set; }

        public event Action OnStatsChanged;

        private void Awake()
        {
            Damage = baseDamage;
            SoulMultiplier = baseSoulMultiplier;
        }

        public void AddDamage(int amount)
        {
            Damage += amount;
            OnStatsChanged?.Invoke();
        }

        public void AddSoulMultiplier(float amount)
        {
            SoulMultiplier += amount;
            OnStatsChanged?.Invoke();
        }
    }
}
