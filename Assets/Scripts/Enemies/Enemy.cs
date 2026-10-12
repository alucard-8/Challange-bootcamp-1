using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Moves toward its target and dies. It does NOT give souls, drop orbs, split or draw effects —
    // it raises events and other systems (wallet, orb dropper, splitter, fx) react.
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour, IShootable, IDamageable
    {
        public static event Action<Enemy> OnAnyKilled;
        public static event Action<Enemy> OnAnyReachedTarget;
        public static int AliveCount { get; private set; }

        public event Action<Enemy> OnDamaged;

        public EnemyConfig Config { get; private set; }
        public Transform Target { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float SpeedMultiplier { get; private set; }
        public float HealthMultiplier { get; private set; }
        public bool InRange { get; private set; }

        private float speed;

        // Clear static state when entering Play Mode with domain reload disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnAnyKilled = null;
            OnAnyReachedTarget = null;
            AliveCount = 0;
        }

        private void OnEnable() => AliveCount++;
        private void OnDisable() => AliveCount--;

        public void Init(EnemyConfig config, Transform target, float healthMultiplier, float speedMultiplier)
        {
            Config = config;
            Target = target;
            HealthMultiplier = healthMultiplier;
            SpeedMultiplier = speedMultiplier;
            MaxHealth = Health = Mathf.Max(1, Mathf.RoundToInt(config.maxHealth * healthMultiplier));
            speed = config.speed * speedMultiplier;
            transform.localScale = Vector3.one * config.size;
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
            OnAnyKilled?.Invoke(this);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Health <= 0 || other.transform != Target) return;
            if (other.TryGetComponent(out IDamageable damageable)) damageable.TakeDamage(Config.damage);

            Health = 0;
            OnAnyReachedTarget?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
