using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [Serializable]
    public class WaveEnemy
    {
        public EnemyConfig config;
        [Min(1)] public int firstWave = 1;
        [Min(0)] public float weight = 1f;
    }

    // Runs waves: spawns a budget of enemies, then reports the wave as cleared once all are dead.
    // It doesn't know the shop exists — GameManager connects "wave cleared" to "open shop".
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private WaveEnemy[] roster;
        [SerializeField] private EnemyConfig bossConfig;
        [SerializeField] private int bossEvery = 5;
        [SerializeField] private float spawnRadius = 11f;

        [Header("Scaling per wave")]
        [SerializeField] private int baseCount = 8;
        [SerializeField] private int countPerWave = 3;
        [SerializeField] private float baseInterval = 0.9f;
        [SerializeField] private float intervalPerWave = 0.05f;
        [SerializeField] private float minInterval = 0.25f;
        [SerializeField] private float healthGrowth = 0.1f;
        [SerializeField] private float speedGrowth = 0.03f;

        public int Wave { get; private set; }
        public bool InCombat { get; private set; }
        public bool IsBossWave => bossConfig != null && bossEvery > 0 && Wave % bossEvery == 0;
        public int RemainingToSpawn { get; private set; }

        public event Action<int, bool> OnWaveStarted;   // (wave, isBoss)
        public event Action<int> OnWaveCleared;

        private float timer;
        private readonly List<WaveEnemy> pool = new List<WaveEnemy>();

        public void StartNextWave()
        {
            Wave++;
            InCombat = true;
            RemainingToSpawn = baseCount + countPerWave * (Wave - 1);
            timer = 1f;

            if (IsBossWave)
            {
                RemainingToSpawn /= 2;
                Spawn(bossConfig);
            }
            OnWaveStarted?.Invoke(Wave, IsBossWave);
        }

        private void Update()
        {
            if (!InCombat) return;

            if (RemainingToSpawn > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = Mathf.Max(minInterval, baseInterval - intervalPerWave * (Wave - 1));
                    Spawn(PickEnemy());
                    RemainingToSpawn--;
                }
            }
            else if (Enemy.AliveCount == 0)
            {
                InCombat = false;
                OnWaveCleared?.Invoke(Wave);
            }
        }

        private EnemyConfig PickEnemy()
        {
            pool.Clear();
            float total = 0f;
            foreach (var e in roster)
            {
                if (e.config == null || e.firstWave > Wave) continue;
                pool.Add(e);
                total += e.weight;
            }

            float roll = UnityEngine.Random.value * total;
            foreach (var e in pool)
            {
                roll -= e.weight;
                if (roll <= 0f) return e.config;
            }
            return pool.Count > 0 ? pool[pool.Count - 1].config : roster[0].config;
        }

        private void Spawn(EnemyConfig config)
        {
            if (config == null || target == null) return;
            Vector2 pos = (Vector2)target.position + UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            float hp = 1f + healthGrowth * (Wave - 1);
            float spd = 1f + speedGrowth * (Wave - 1);
            EnemyFactory.Create(config, pos, target, hp, spd);
        }
    }
}
