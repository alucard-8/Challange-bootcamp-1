using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Economy
{
    // Observer: pays interest on banked souls whenever a wave is cleared (unlocked by perks).
    public class SoulInterest : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Transform payoutPoint;

        private void OnEnable() => waves.OnWaveCleared += HandleWaveCleared;
        private void OnDisable() => waves.OnWaveCleared -= HandleWaveCleared;

        private void HandleWaveCleared(int wave)
        {
            int interest = Mathf.FloorToInt(wallet.Souls * stats.Get(StatType.Interest));
            wallet.Add(interest, payoutPoint != null ? payoutPoint.position : Vector3.zero);
        }
    }
}
