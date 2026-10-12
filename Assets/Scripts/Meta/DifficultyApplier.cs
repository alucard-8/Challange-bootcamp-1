using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Applies the chosen difficulty to the run before anything starts (like ModeApplier, a mediator).
    [DefaultExecutionOrder(-210)]
    public class DifficultyApplier : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private Reactor.Reactor reactor;

        private void Awake()
        {
            var d = DifficultyTable.Current;
            waves.SetDifficulty(d.EnemyHealth, d.EnemySpeed, d.EnemyCount, d.EliteChance);
            reactor.SetHealthMultiplier(d.CoreHealth);
        }
    }
}
