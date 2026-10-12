using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Moves toward its target and dies. It does NOT give souls, drop orbs, split or draw effects —
    // it raises events on its channels and other systems (wallet, orb dropper, splitter, fx) react.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour, IHostile, IDamageable, IWeakPoint
    {
        public event Action<Enemy> OnDamaged;   // per-instance, for this enemy's own view

        public EnemyConfig Config { get; private set; }
        public EnemyChannels Channels { get; private set; }
        public EnemyModifier Modifiers { get; private set; }
        public Transform Target { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int ShieldHits { get; private set; }
        public float SpeedMultiplier { get; private set; }
        public float HealthMultiplier { get; private set; }
        public bool InRange { get; private set; }

        public bool IsAlive => Health > 0;
        public bool IsHeavy => Config != null && Config.isBoss;
        public bool IsElite => Modifiers != EnemyModifier.None;
        public float RewardMultiplier => IsElite ? 3f : 1f;

        private float speed;
        private float speedBoost = 1f;

        public void Init(EnemyConfig config, Transform target, EnemyChannels channels, float healthMultiplier,
                         float speedMultiplier, EnemyModifier modifiers = EnemyModifier.None)
        {
            Config = config;
            Target = target;
            Channels = channels;
            Modifiers = modifiers;
            HealthMultiplier = healthMultiplier;
            SpeedMultiplier = speedMultiplier;
            MaxHealth = Health = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * healthMultiplier * (IsElite ? 2f : 1f)));
            ShieldHits = Has(EnemyModifier.Shielded) ? 3 : 0;
            speed = config.speed * speedMultiplier * (Has(EnemyModifier.Swift) ? 1.7f : 1f);
            transform.localScale = Vector3.one * config.size * (IsElite ? 1.15f : 1f);

            if (channels?.spawned != null) channels.spawned.Raise(this);
        }

        public bool Has(EnemyModifier m) => (Modifiers & m) != 0;

        // Used by the boss when it enrages.
        public void BoostSpeed(float multiplier) => speedBoost = multiplier;

        private void Update()
        {
            if (Target == null) return;

            Vector3 toTarget = Target.position - transform.position;
            InRange = Config.stopDistance > 0f && toTarget.magnitude <= Config.stopDistance;
            if (!InRange)
                transform.position = Vector3.MoveTowards(transform.position, Target.position, speed * speedBoost * Time.deltaTime);
        }

        // The glowing eye in the middle of the shader is the weak point.
        public bool IsWeakPointHit(Vector2 worldPoint) =>
            Vector2.Distance(worldPoint, transform.position) <= transform.localScale.x * 0.17f;

        public void Shoot(int damage) => TakeDamage(damage);

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;

            if (ShieldHits > 0)
            {
                ShieldHits--;
                OnDamaged?.Invoke(this);
                return;
            }

            Health -= amount;
            OnDamaged?.Invoke(this);

            if (Health > 0) return;
            if (Channels?.killed != null) Channels.killed.Raise(this);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Health <= 0 || other.transform != Target) return;
            if (other.TryGetComponent(out IDamageable damageable)) damageable.TakeDamage(Config.damage);

            Health = 0;
            if (Channels?.reachedTarget != null) Channels.reachedTarget.Raise(this);
            Destroy(gameObject);
        }
    }
}
