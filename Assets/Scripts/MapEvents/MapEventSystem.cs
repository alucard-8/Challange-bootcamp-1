using System.Collections;
using Soulbreak.Core.Events;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.MapEvents
{
    // Random arena events rolled at the start of waves (from wave 3):
    //   SOUL URN     — a treasure urn drifts across; crack it before it escapes for a soul jackpot.
    //   SOLAR FLARE  — the reactor shield goes offline for a while.
    //   REJUVENATOR  — a crystal golem joins the wave; kill it for a temporary damage + reload buff.
    public class MapEventSystem : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private AnnouncementEventChannel announcements;

        [Header("Tuning")]
        [SerializeField] private int firstWave = 3;
        [SerializeField, Range(0f, 1f)] private float chancePerWave = 0.5f;

        [Header("Soul Urn")]
        [SerializeField] private Sprite urnSprite;
        [SerializeField] private Material urnMaterial;
        [SerializeField] private int urnHits = 10;
        [SerializeField] private int urnJackpot = 150;

        [Header("Solar Flare")]
        [SerializeField] private float flareDuration = 10f;

        [Header("Rejuvenator")]
        [SerializeField] private EnemyConfig rejuvenatorConfig;
        [SerializeField] private float buffDuration = 20f;

        public void SetRejuvenator(EnemyConfig config) => rejuvenatorConfig = config;

        private static readonly Color Gold = new Color(1f, 0.8f, 0.3f);
        private static readonly Color Orange = new Color(1f, 0.5f, 0.15f);
        private static readonly Color Green = new Color(0.4f, 1f, 0.6f);

        private void OnEnable()
        {
            waves.OnWaveStarted += HandleWaveStarted;
            enemyKilled.OnRaised += HandleKilled;
        }

        private void OnDisable()
        {
            waves.OnWaveStarted -= HandleWaveStarted;
            enemyKilled.OnRaised -= HandleKilled;
        }

        private void HandleWaveStarted(int wave, bool boss)
        {
            if (wave < firstWave || boss || Random.value > chancePerWave) return;
            switch (Random.Range(0, 3))
            {
                case 0: StartCoroutine(SoulUrnEvent()); break;
                case 1: StartCoroutine(SolarFlareEvent()); break;
                case 2: StartCoroutine(RejuvenatorEvent()); break;
            }
        }

        // ---------- Soul Urn ----------
        private IEnumerator SoulUrnEvent()
        {
            yield return new WaitForSeconds(3f);
            announcements.Raise(new Announcement("SOUL URN", $"Crack it before it escapes!  {urnHits} hits", Gold));

            float side = Random.value < 0.5f ? -1f : 1f;
            Vector2 start = new Vector2(10.5f * side, Random.Range(-3.5f, 3.5f));
            Vector2 velocity = new Vector2(-side * 1.6f, Random.Range(-0.3f, 0.3f));
            var urn = SoulUrn.Spawn(start, velocity, urnHits, 13f, urnSprite, urnMaterial);
            urn.OnHit += u => wallet.Add(2, u.transform.position);
            urn.OnBroken += u =>
            {
                wallet.Add(urnJackpot, u.transform.position);
                announcements.Raise(new Announcement("JACKPOT", $"+{urnJackpot} SOULS", Gold));
            };
        }

        // ---------- Solar Flare ----------
        private IEnumerator SolarFlareEvent()
        {
            yield return new WaitForSeconds(2f);
            announcements.Raise(new Announcement("SOLAR FLARE", "Incoming! Shield going offline...", Orange));
            yield return new WaitForSeconds(2f);
            reactor.SetShieldSuppressed(true);
            yield return new WaitForSeconds(flareDuration);
            reactor.SetShieldSuppressed(false);
            announcements.Raise(new Announcement("SHIELD ONLINE", "", new Color(0.45f, 0.85f, 1f)));
        }

        // ---------- Rejuvenator ----------
        private IEnumerator RejuvenatorEvent()
        {
            if (rejuvenatorConfig == null) yield break;
            yield return new WaitForSeconds(4f);
            announcements.Raise(new Announcement("REJUVENATOR", "Destroy the crystal golem for a power surge", Green));
            waves.SpawnExtra(rejuvenatorConfig);
        }

        private void HandleKilled(Enemy e)
        {
            if (rejuvenatorConfig == null || e.Config != rejuvenatorConfig) return;
            StartCoroutine(PowerSurge());
        }

        private IEnumerator PowerSurge()
        {
            announcements.Raise(new Announcement("POWER SURGE", $"+50% damage, faster reloads for {buffDuration:0}s", Green));
            stats.Multiply(StatType.Damage, 1.5f);
            stats.Multiply(StatType.ReloadTime, 0.6f);
            yield return new WaitForSeconds(buffDuration);
            stats.Multiply(StatType.Damage, 1f / 1.5f);
            stats.Multiply(StatType.ReloadTime, 1f / 0.6f);
        }
    }
}
