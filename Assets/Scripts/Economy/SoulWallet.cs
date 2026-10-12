using System;
using Soulbreak.Enemies;
using Soulbreak.Orbs;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Economy
{
    // Observer: earns souls from kills and soul orbs. Owns the soul count and nothing else.
    public class SoulWallet : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;

        public int Souls { get; private set; }
        public event Action<int> OnSoulsChanged;

        private void OnEnable()
        {
            Enemy.OnAnyKilled += HandleEnemyKilled;
            Orb.OnAnyCollected += HandleOrbCollected;
        }

        private void OnDisable()
        {
            Enemy.OnAnyKilled -= HandleEnemyKilled;
            Orb.OnAnyCollected -= HandleOrbCollected;
        }

        private void Start() => OnSoulsChanged?.Invoke(Souls);

        private void HandleEnemyKilled(Enemy enemy) => Add(enemy.Config.soulReward);

        private void HandleOrbCollected(Orb orb)
        {
            if (orb.Type == OrbType.Soul) Add(orb.Value);
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Souls += Mathf.RoundToInt(amount * stats.SoulMultiplier);
            OnSoulsChanged?.Invoke(Souls);
        }

        public bool TrySpend(int amount)
        {
            if (amount > Souls) return false;
            Souls -= amount;
            OnSoulsChanged?.Invoke(Souls);
            return true;
        }
    }
}
