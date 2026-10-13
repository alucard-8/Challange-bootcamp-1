using System;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.Meta
{
    public struct RunResult
    {
        public int Score, Essence, Wave, WavesCleared, Kills, EliteKills, BossKills, Headshots, BestStreak, SoulsEarned;
        public float Time;
        public bool NewBest;
        public int PreviousBest;
        public Difficulty Difficulty;
    }

    public class MetaRecorder : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private WaveManager waves;
        [SerializeField] private RunStats stats;

        public event Action<RunResult> OnRunRecorded;
        public RunResult Last { get; private set; }

        private void OnEnable() => game.OnStateChanged += HandleState;
        private void OnDisable() => game.OnStateChanged -= HandleState;

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) return;

            var diff = DifficultyTable.Current;
            int raw = stats.Kills * 10 + stats.EliteKills * 40 + stats.BossKills * 500 + stats.Headshots * 15
                      + stats.BestStreak * 20 + game.WavesCleared * 150 + wallet.TotalEarned;
            int score = Mathf.RoundToInt(raw * diff.ScoreMultiplier);
            int essence = Mathf.RoundToInt((wallet.TotalEarned / 10 + game.WavesCleared * 5) * diff.ScoreMultiplier);

            int previousBest = MetaProgress.BestScore;
            MetaProgress.AddEssence(essence);
            bool best = MetaProgress.RecordRun(score, waves.Wave);

            Last = new RunResult
            {
                Score = score, Essence = essence, Wave = waves.Wave, WavesCleared = game.WavesCleared,
                Kills = stats.Kills, EliteKills = stats.EliteKills, BossKills = stats.BossKills, Headshots = stats.Headshots,
                BestStreak = stats.BestStreak, SoulsEarned = wallet.TotalEarned, Time = game.SurvivalTime,
                NewBest = best, PreviousBest = previousBest, Difficulty = MetaProgress.SelectedDifficulty,
            };
            OnRunRecorded?.Invoke(Last);
        }
    }
}
