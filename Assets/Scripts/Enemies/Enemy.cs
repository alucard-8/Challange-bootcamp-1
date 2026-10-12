using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Moves toward its target and dies. It does NOT give souls or drop orbs itself —
    // it just raises static events and other systems (wallet, orb dropper) react.
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
    public class Enemy : MonoBehaviour, IShootable, IDamageable
    {
        public static event Action<Enemy> OnAnyKilled;
        public static event Action<Enemy> OnAnyReachedTarget;

        public EnemyConfig Config { get; private set; }
        public int Health { get; private set; }

        private Transform target;
        private float speed;
        private SpriteRenderer sr;
        private float flashTimer;

        // Clear static subscribers when entering Play Mode with domain reload disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnAnyKilled = null;
            OnAnyReachedTarget = null;
        }

        public void Init(EnemyConfig config, Transform target, float speedMultiplier)
        {
            Config = config;
            this.target = target;
            Health = config.maxHealth;
            speed = config.speed * speedMultiplier;

            sr = GetComponent<SpriteRenderer>();
            sr.sprite = config.sprite;
            sr.color = config.color;
            transform.localScale = Vector3.one * config.size;
        }

        private void Update()
        {
            if (target == null) return;
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (flashTimer > 0)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0) sr.color = Config.color;
            }
        }

        public void Shoot(int damage) => TakeDamage(damage);

        public void TakeDamage(int amount)
        {
            if (Health <= 0) return;
            Health -= amount;

            if (Health <= 0)
            {
                OnAnyKilled?.Invoke(this);
                Destroy(gameObject);
                return;
            }

            sr.color = Color.white;
            flashTimer = 0.08f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Health <= 0 || other.transform != target) return;
            if (other.TryGetComponent(out IDamageable damageable)) damageable.TakeDamage(Config.damage);

            Health = 0;
            OnAnyReachedTarget?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
