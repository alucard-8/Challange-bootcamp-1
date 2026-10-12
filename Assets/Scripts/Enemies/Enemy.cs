using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Moves toward its target and dies. It does NOT give souls, drop orbs, split or draw effects —
    // it raises events on its channels and other systems (wallet, orb dropper, splitter, fx) react.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour, IHostile, IDamageable
    {
        public event Action<Enemy> OnDamaged;   // per-instance, for this enemy's own view

        public EnemyConfig Config { get; private set; }
        public EnemyChannels Channels { get; private set; }
        public Transform Target { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float SpeedMultiplier { get; private set; }
        public float HealthMultiplier { get; private set; }
        public bool InRange { get; private set; }

        public bool IsAlive => Health > 0;
        public bool IsHeavy => Config != null && Config.isBoss;

        private float speed;

        public void Init(EnemyConfig config, Transform target, EnemyChannels channels, float healthMultiplier, float speedMultiplier)
        {
            Config = config;
            Target = target;
            Channels = channels;
            HealthMultiplier = healthMultiplier;
            SpeedMultiplier = speedMultiplier;
            MaxHealth = Health = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * healthMultiplier));
            speed = config.speed * speedMultiplier;
            transform.localScale = Vector3.one * config.size;

            if (channels?.spawned != null) channels.spawned.Raise(this);
        }

        private void Update()
        {
            if (Target == null) return;

            Vector3 toTarget = Target.position - transform.position;
            InRange = Config.stopDistance > 0f && toTarget.magnitude <= Config.stopDistance;
            if (!InRange)
                transform.position = Vector3.MoveTowards(transform.position, Target.position, speed * Time.deltaTime);
        }

        public void Shoot(int damage) => TakeDamage(damage);

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;
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
