using System;
using System.Collections.Generic;
using Soulbreak.Core.Events;
using Soulbreak.Economy;
using Soulbreak.Enemies;
using Soulbreak.Orbs;
using Soulbreak.Perks;
using Soulbreak.Player;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Audio
{
    [Serializable]
    public class PatternSound
    {
        public ShotPattern pattern;
        public Sfx sound;
    }

    // Pure observer: turns gameplay events into sound effects. Nothing in gameplay knows audio exists.
    // Uses a small pool of AudioSources, slight random pitch, and a per-sound cooldown to avoid spam.
    public class AudioDirector : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private OverdriveSystem overdrive;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private WaveManager waves;
        [SerializeField] private PerkShop shop;
        [SerializeField] private ComboSystem combo;

        [Header("Channels")]
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private EnemyEventChannel bossEnraged;
        [SerializeField] private OrbEventChannel orbCollected;
        [SerializeField] private EnemyProjectileEventChannel projectileShotDown;
        [SerializeField] private ExplosionEventChannel explosions;
        [SerializeField] private Vector2EventChannel headshots;
        [SerializeField] private AnnouncementEventChannel announcements;

        [Header("Weapon sounds")]
        [SerializeField] private PatternSound[] patternSounds;
        [SerializeField, Range(0f, 1f)] private float volume = 0.6f;

        private readonly List<AudioSource> pool = new List<AudioSource>();
        private readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
        private int next;
        private bool alarmPlayed;

        private void Awake()
        {
            for (int i = 0; i < 16; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                pool.Add(s);
            }
            // Pre-build every clip now so the first shot doesn't hitch.
            foreach (Sfx id in Enum.GetValues(typeof(Sfx))) SfxSynth.Get(id);
        }

        private void OnEnable()
        {
            shooter.OnShotFired += HandleShot;
            ammo.OnReloadStarted += HandleReload;
            ammo.OnDryFire += HandleDryFire;
            ammo.OnAmmoPickup += HandleAmmoPickup;
            nova.OnUsed += HandleNova;
            overdrive.OnStarted += HandleOverdrive;
            reactor.OnHit += HandleReactorHit;
            reactor.OnHealthChanged += HandleHealth;
            waves.OnWaveStarted += HandleWaveStarted;
            waves.OnWaveCleared += HandleWaveCleared;
            shop.OnPerkBought += HandleBought;
            shop.OnPurchaseFailed += HandleFailed;
            combo.OnMilestone += HandleMilestone;
            enemyKilled.OnRaised += HandleKill;
            bossEnraged.OnRaised += HandleEnrage;
            orbCollected.OnRaised += HandleOrb;
            projectileShotDown.OnRaised += HandleShotDown;
            explosions.OnRaised += HandleExplosion;
            headshots.OnRaised += HandleHeadshot;
            announcements.OnRaised += HandleAnnouncement;
        }

        private void OnDisable()
        {
            shooter.OnShotFired -= HandleShot;
            ammo.OnReloadStarted -= HandleReload;
            ammo.OnDryFire -= HandleDryFire;
            ammo.OnAmmoPickup -= HandleAmmoPickup;
            nova.OnUsed -= HandleNova;
            overdrive.OnStarted -= HandleOverdrive;
            reactor.OnHit -= HandleReactorHit;
            reactor.OnHealthChanged -= HandleHealth;
            waves.OnWaveStarted -= HandleWaveStarted;
            waves.OnWaveCleared -= HandleWaveCleared;
            shop.OnPerkBought -= HandleBought;
            shop.OnPurchaseFailed -= HandleFailed;
            combo.OnMilestone -= HandleMilestone;
            enemyKilled.OnRaised -= HandleKill;
            bossEnraged.OnRaised -= HandleEnrage;
            orbCollected.OnRaised -= HandleOrb;
            projectileShotDown.OnRaised -= HandleShotDown;
            explosions.OnRaised -= HandleExplosion;
            headshots.OnRaised -= HandleHeadshot;
            announcements.OnRaised -= HandleAnnouncement;
        }

        // ---------- handlers ----------
        private void HandleShot(Vector2 from, Vector2 to, bool crit, ShotPattern pattern) => Play(SoundFor(pattern), crit ? 1f : 0.8f);
        private void HandleReload(float duration) => Play(Sfx.Reload, 0.6f);
        private void HandleAmmoPickup() { Play(Sfx.Reload, 0.9f); Play(Sfx.Buy, 0.5f); }
        private void HandleDryFire() => Play(Sfx.Click, 0.6f);
        private void HandleNova(Vector2 c, float r) => Play(Sfx.Nova, 1f, 0f);
        private void HandleOverdrive() => Play(Sfx.Overdrive, 1f, 0f);
        private void HandleReactorHit(bool shielded) => Play(shielded ? Sfx.ShieldHit : Sfx.ReactorHit, 1f);
        private void HandleWaveStarted(int wave, bool boss) => Play(boss ? Sfx.Enrage : Sfx.WaveStart, 0.8f, 0f);
        private void HandleWaveCleared(int wave) => Play(Sfx.WaveClear, 0.8f, 0f);
        private void HandleBought(PerkDefinition perk) => Play(Sfx.Buy, 0.8f, 0f);
        private void HandleFailed() => Play(Sfx.Fail, 0.6f, 0f);
        private void HandleMilestone(int count, string title) => Play(Sfx.Streak, 0.9f, 0f);
        private void HandleKill(Enemy e) => Play(e.Config.isBoss || e.IsElite || e.Config.size >= 1.6f ? Sfx.BigKill : Sfx.Kill, 0.7f);
        private void HandleEnrage(Enemy e) => Play(Sfx.Enrage, 1f, 0f);
        private void HandleOrb(Orb o) => Play(Sfx.Orb, 0.7f);
        private void HandleShotDown(EnemyProjectile p) => Play(Sfx.Click, 0.7f);
        private void HandleExplosion(ExplosionInfo e) => Play(Sfx.Explosion, Mathf.Clamp01(0.5f + e.Radius * 0.2f));
        private void HandleHeadshot(Vector2 p) => Play(Sfx.Headshot, 0.9f, 0.05f);
        private void HandleAnnouncement(Announcement a) => Play(Sfx.Streak, 0.5f, 0f);

        private void HandleHealth(int cur, int max)
        {
            bool low = max > 0 && cur > 0 && cur <= max * 0.3f;
            if (low && !alarmPlayed) Play(Sfx.Alarm, 0.8f, 0f);
            alarmPlayed = low;
        }

        private Sfx SoundFor(ShotPattern pattern)
        {
            if (patternSounds != null)
                foreach (var ps in patternSounds)
                    if (ps.pattern == pattern) return ps.sound;
            return Sfx.Laser;
        }

        private void Play(Sfx id, float vol, float pitchVariance = 0.08f)
        {
            if (lastPlayed.TryGetValue(id, out float last) && Time.unscaledTime - last < 0.035f) return;
            lastPlayed[id] = Time.unscaledTime;

            var src = pool[next];
            next = (next + 1) % pool.Count;
            src.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
            src.PlayOneShot(SfxSynth.Get(id), vol * volume * Core.GameSettings.Sfx);
        }
    }
}
