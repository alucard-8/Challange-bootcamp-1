using UnityEngine;

namespace Soulbreak.Enemies
{
    // Two-phase boss behaviour, added by the factory to boss enemies.
    // Phase 1: summons minion packs. Phase 2 (below 50% health): enrages — faster, and fires
    // rings of bullets at the reactor that the player must shoot down.
    [RequireComponent(typeof(Enemy))]
    public class BossBrain : MonoBehaviour
    {
        [SerializeField] private float summonInterval = 6f;
        [SerializeField] private int minionsPerSummon = 4;
        [SerializeField] private float volleyInterval = 1.5f;
        [SerializeField] private int bulletsPerVolley = 7;

        public bool Enraged { get; private set; }

        private Enemy enemy;
        private float summonTimer, volleyTimer;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            summonTimer = 3f;
            volleyTimer = 1f;
        }

        private void Update()
        {
            if (enemy.Target == null || !enemy.IsAlive) return;

            if (!Enraged && enemy.Health <= enemy.MaxHealth / 2) Enrage();

            if (!Enraged)
            {
                summonTimer -= Time.deltaTime;
                if (summonTimer <= 0f) { summonTimer = summonInterval; Summon(); }
            }
            else
            {
                volleyTimer -= Time.deltaTime;
                if (volleyTimer <= 0f) { volleyTimer = volleyInterval; Volley(); }
            }
        }

        private void Enrage()
        {
            Enraged = true;
            enemy.BoostSpeed(1.5f);
            if (enemy.Channels?.bossEnraged != null) enemy.Channels.bossEnraged.Raise(enemy);
        }

        private void Summon()
        {
            var minion = enemy.Config.minion;
            if (minion == null) return;
            for (int i = 0; i < minionsPerSummon; i++)
            {
                float a = (i / (float)minionsPerSummon) * Mathf.PI * 2f;
                Vector2 pos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.6f;
                EnemyFactory.Create(minion, pos, enemy.Target, enemy.Channels, enemy.HealthMultiplier, enemy.SpeedMultiplier);
            }
        }

        private void Volley()
        {
            float offset = Random.value * 360f;
            for (int i = 0; i < bulletsPerVolley; i++)
            {
                float a = (offset + i * 360f / bulletsPerVolley) * Mathf.Deg2Rad;
                Vector2 pos = (Vector2)transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * transform.localScale.x * 0.45f;
                EnemyProjectile.Spawn(pos, enemy.Target, enemy.Config, enemy.Channels?.projectileShotDown);
            }
        }
    }
}
