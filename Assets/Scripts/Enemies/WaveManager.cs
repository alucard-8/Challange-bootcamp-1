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

    // Runs waves: spawns a budget of enemies (some rolled as elites), then reports the wave as cleared
    // once the EnemyRegistry is empty. It doesn't know the shop exists — GameManager connects them.
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Channels handed to every enemy this manager spawns.")]
        [SerializeField] private EnemyChannels channels;
        [SerializeField] private EnemyRegistry registry;
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

        [Header("Elites")]
        [SerializeField] private int eliteFirstWave = 3;
        [SerializeField] private float eliteBaseChance = 0.06f;
        [SerializeField] private float eliteChancePerWave = 0.02f;
        [SerializeField] private float eliteMaxChance = 0.35f;

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
            RemainingToSpawn = Mathf.Max(1, Mathf.RoundToInt((baseCount + countPerWave * (Wave - 1)) * diffCount));
            timer = 1f;

            if (IsBossWave)
            {
                RemainingToSpawn /= 2;
                Spawn(bossConfig, EnemyModifier.None);
            }
            OnWaveStarted?.Invoke(Wave, IsBossWave);
        }

        // Configuration (used by the mode applier before the first wave).
        public void SetRoster(WaveEnemy[] newRoster, EnemyConfig newBoss) { roster = newRoster; bossConfig = newBoss; }

        private float diffHealth = 1f, diffSpeed = 1f, diffCount = 1f, diffElite = 1f;
        public void SetDifficulty(float health, float speed, float count, float elite) { diffHealth = health; diffSpeed = speed; diffCount = count; diffElite = elite; }

        // Lets other systems (map events) add an enemy to the current wave.
        public Enemy SpawnExtra(EnemyConfig config) => Spawn(config, EnemyModifier.None);

        private void Update()
        {
            if (!InCombat) return;

            if (RemainingToSpawn > 0)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = Mathf.Max(minInterval, baseInterval - intervalPerWave * (Wave - 1));
                    Spawn(PickEnemy(), RollModifiers());
                    RemainingToSpawn--;
                }
            }
            else if (registry.Count == 0)
            {
                InCombat = false;
                OnWaveCleared?.Invoke(Wave);
            }
        }

        private EnemyModifier RollModifiers()
        {
            if (Wave < eliteFirstWave) return EnemyModifier.None;
            float chance = Mathf.Min(eliteMaxChance * diffElite, (eliteBaseChance + eliteChancePerWave * (Wave - eliteFirstWave)) * diffElite);
            if (UnityEngine.Random.value > chance) return EnemyModifier.None;

            var all = EnemyModifierInfo.All;
            var mods = all[UnityEngine.Random.Range(0, all.Length)];
            if (Wave >= 10 && UnityEngine.Random.value < 0.35f) mods |= all[UnityEngine.Random.Range(0, all.Length)];
            return mods;
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

        private Enemy Spawn(EnemyConfig config, EnemyModifier mods)
        {
            if (config == null || target == null) return null;
            Vector2 pos = (Vector2)target.position + UnityEngine.Random.insideUnitCircle.normalized * spawnRadius;
            float hp = (1f + healthGrowth * (Wave - 1)) * diffHealth;
            float spd = (1f + speedGrowth * (Wave - 1)) * diffSpeed;
            return EnemyFactory.Create(config, pos, target, channels, hp, spd, mods);
        }
    }
}
