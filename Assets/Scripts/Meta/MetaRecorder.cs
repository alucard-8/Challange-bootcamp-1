using System;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Observer: when the run ends, converts it into Soul Essence (10% of souls earned + 5 per wave)
    // and records the best wave.
    public class MetaRecorder : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private WaveManager waves;

        public event Action<int> OnEssenceAwarded;
        public int LastAward { get; private set; }

        private void OnEnable() => game.OnStateChanged += HandleState;
        private void OnDisable() => game.OnStateChanged -= HandleState;

        private void HandleState(GameState state)
        {
            if (state != GameState.GameOver) return;
            LastAward = wallet.TotalEarned / 10 + game.WavesCleared * 5;
            MetaProgress.AddEssence(LastAward);
            MetaProgress.RecordWave(waves.Wave);
            OnEssenceAwarded?.Invoke(LastAward);
        }
    }
}
