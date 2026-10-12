using System;
using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Economy
{
    // Observer: chains fast kills into a combo that boosts soul rewards.
    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private float bonusPerKill = 0.05f;
        [SerializeField] private float maxMultiplier = 2f;

        public int Count { get; private set; }
        public float Multiplier => Mathf.Min(maxMultiplier, 1f + Count * bonusPerKill);
        public float TimeLeft01 => window > 0f ? timer / window : 0f;

        public event Action<int, float> OnComboChanged;   // (count, multiplier)

        private float timer, window;

        private void OnEnable() => Enemy.OnAnyKilled += HandleKill;
        private void OnDisable() => Enemy.OnAnyKilled -= HandleKill;

        private void HandleKill(Enemy enemy)
        {
            Count++;
            window = timer = stats.Get(StatType.ComboWindow);
            OnComboChanged?.Invoke(Count, Multiplier);
        }

        private void Update()
        {
            if (Count == 0) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;

            Count = 0;
            OnComboChanged?.Invoke(Count, Multiplier);
        }
    }
}
