using UnityEngine;

namespace Soulbreak.Enemies
{
    // Observer: when an enemy whose config has splitInto dies, spawn its children.
    public class EnemySplitter : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private float spread = 0.6f;

        private void OnEnable() => enemyKilled.OnRaised += HandleKilled;
        private void OnDisable() => enemyKilled.OnRaised -= HandleKilled;

        private void HandleKilled(Enemy enemy)
        {
            var cfg = enemy.Config;
            if (cfg.splitInto == null || cfg.splitCount <= 0) return;

            for (int i = 0; i < cfg.splitCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spread;
                EnemyFactory.Create(cfg.splitInto, (Vector2)enemy.transform.position + offset, enemy.Target,
                                    enemy.Channels, enemy.HealthMultiplier, enemy.SpeedMultiplier);
            }
        }
    }
}
