using Soulbreak.Core.Events;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    public class RunStats : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private Vector2EventChannel headshots;
        [SerializeField] private ComboSystem combo;

        public int Kills { get; private set; }
        public int EliteKills { get; private set; }
        public int BossKills { get; private set; }
        public int Headshots { get; private set; }
        public int BestStreak { get; private set; }

        private void OnEnable()
        {
            enemyKilled.OnRaised += HandleKill;
            headshots.OnRaised += HandleHeadshot;
            combo.OnComboChanged += HandleCombo;
        }

        private void OnDisable()
        {
            enemyKilled.OnRaised -= HandleKill;
            headshots.OnRaised -= HandleHeadshot;
            combo.OnComboChanged -= HandleCombo;
        }

        private void HandleKill(Enemy e)
        {
            Kills++;
            if (e.IsElite) EliteKills++;
            if (e.Config.isBoss) BossKills++;
        }

        private void HandleHeadshot(Vector2 p) => Headshots++;
        private void HandleCombo(int count, float mult) => BestStreak = Mathf.Max(BestStreak, count);
    }
}
