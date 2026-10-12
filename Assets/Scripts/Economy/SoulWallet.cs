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
        [SerializeField] private ComboSystem combo;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private OrbEventChannel orbCollected;
        [SerializeField] private int startingSouls = 0;

        public int Souls { get; private set; }

        public event Action<int> OnSoulsChanged;
        public event Action<int, Vector3> OnSoulsGained;   // (amount, world position) — for floating text

        private void Awake() => Souls = startingSouls;

        private void OnEnable()
        {
            enemyKilled.OnRaised += HandleEnemyKilled;
            orbCollected.OnRaised += HandleOrbCollected;
        }

        private void OnDisable()
        {
            enemyKilled.OnRaised -= HandleEnemyKilled;
            orbCollected.OnRaised -= HandleOrbCollected;
        }

        private void Start() => OnSoulsChanged?.Invoke(Souls);

        private void HandleEnemyKilled(Enemy enemy)
        {
            float amount = enemy.Config.soulReward * stats.Get(StatType.SoulGain) * combo.Multiplier;
            Add(Mathf.RoundToInt(amount), enemy.transform.position);
        }

        private void HandleOrbCollected(Orb orb)
        {
            if (orb.Type != OrbType.Soul) return;
            Add(Mathf.RoundToInt(orb.Value * stats.Get(StatType.SoulGain)), orb.transform.position);
        }

        public void Add(int amount, Vector3 worldPosition)
        {
            if (amount <= 0) return;
            Souls += amount;
            OnSoulsChanged?.Invoke(Souls);
            OnSoulsGained?.Invoke(amount, worldPosition);
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
