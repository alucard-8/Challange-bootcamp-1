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
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private float bonusPerKill = 0.05f;
        [SerializeField] private float maxMultiplier = 2f;

        public int Count { get; private set; }
        public float Multiplier => Mathf.Min(maxMultiplier, 1f + Count * bonusPerKill);
        public float TimeLeft01 => window > 0f ? timer / window : 0f;

        public event Action<int, float> OnComboChanged;   // (count, multiplier)
        public event Action<int, string> OnMilestone;     // (count, title) — kill streak announcements

        private static readonly int[] MilestoneCounts = { 5, 10, 15, 25, 40, 60 };
        private static readonly string[] MilestoneTitles = { "KILLING SPREE", "RAMPAGE", "UNSTOPPABLE", "GODLIKE", "SOUL REAPER", "ASCENDED" };

        private float timer, window;

        private void OnEnable() => enemyKilled.OnRaised += HandleKill;
        private void OnDisable() => enemyKilled.OnRaised -= HandleKill;

        private void HandleKill(Enemy enemy)
        {
            Count++;
            window = timer = stats.Get(StatType.ComboWindow);
            OnComboChanged?.Invoke(Count, Multiplier);

            int m = Array.IndexOf(MilestoneCounts, Count);
            if (m >= 0) OnMilestone?.Invoke(Count, MilestoneTitles[m]);
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
