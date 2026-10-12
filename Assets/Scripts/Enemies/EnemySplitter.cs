using UnityEngine;

namespace Soulbreak.Enemies
{
    // Observer: when an enemy that splits dies (its config has splitInto, or it's a "Splitting" elite),
    // spawn its children.
    public class EnemySplitter : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private EnemyConfig eliteSplitInto;
        [SerializeField] private int eliteSplitCount = 3;
        [SerializeField] private float spread = 0.6f;

        private void OnEnable() => enemyKilled.OnRaised += HandleKilled;
        private void OnDisable() => enemyKilled.OnRaised -= HandleKilled;

        private void HandleKilled(Enemy enemy)
        {
            var cfg = enemy.Config;
            if (cfg.splitInto != null && cfg.splitCount > 0) Split(enemy, cfg.splitInto, cfg.splitCount);
            if (enemy.Has(EnemyModifier.Splitting) && eliteSplitInto != null) Split(enemy, eliteSplitInto, eliteSplitCount);
        }

        private void Split(Enemy enemy, EnemyConfig child, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle.normalized * spread;
                EnemyFactory.Create(child, (Vector2)enemy.transform.position + offset, enemy.Target,
                                    enemy.Channels, enemy.HealthMultiplier, enemy.SpeedMultiplier);
            }
        }
    }
}
