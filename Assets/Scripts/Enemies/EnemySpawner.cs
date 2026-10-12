using System;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Spawns enemies on a ring around the target. Gets faster and unlocks tougher types over time.
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Ordered from weakest to strongest. Higher difficulty levels unlock later entries.")]
        [SerializeField] private EnemyConfig[] enemyTypes;
        [SerializeField] private float spawnRadius = 11f;

        [Header("Difficulty")]
        [SerializeField] private float startInterval = 1.4f;
        [SerializeField] private float minInterval = 0.3f;
        [SerializeField] private float intervalDecayPerSecond = 0.012f;
        [SerializeField] private float secondsPerLevel = 20f;
        [SerializeField] private float speedGainPerLevel = 0.12f;

        public int DifficultyLevel { get; private set; }
        public event Action<int> OnDifficultyChanged;

        private float elapsed;
        private float timer;

        private void Start() => OnDifficultyChanged?.Invoke(DifficultyLevel);

        private void Update()
        {
            elapsed += Time.deltaTime;
            timer -= Time.deltaTime;

            int level = Mathf.FloorToInt(elapsed / secondsPerLevel);
            if (level != DifficultyLevel)
            {
                DifficultyLevel = level;
                OnDifficultyChanged?.Invoke(DifficultyLevel);
            }

            if (timer > 0) return;
            timer = Mathf.Max(minInterval, startInterval - intervalDecayPerSecond * elapsed);
            Spawn();
        }

        private void Spawn()
        {
            if (enemyTypes == null || enemyTypes.Length == 0 || target == null) return;

            int unlocked = Mathf.Min(DifficultyLevel + 1, enemyTypes.Length);
            var config = enemyTypes[UnityEngine.Random.Range(0, unlocked)];
            Vector2 pos = (Vector2)target.position + UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;

            EnemyFactory.Create(config, pos, target, 1f + DifficultyLevel * speedGainPerLevel);
        }
    }
}
