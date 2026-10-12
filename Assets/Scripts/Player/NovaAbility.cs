using System;
using Soulbreak.Core;
using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Player
{
    // Panic button: SPACE blasts every enemy near the reactor, then goes on cooldown.
    public class NovaAbility : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private int damage = 99;

        public float CooldownRemaining { get; private set; }
        public float Cooldown => stats.Get(StatType.NovaCooldown);
        public float Radius => stats.Get(StatType.NovaRadius);
        public bool IsReady => CooldownRemaining <= 0f;

        public event Action<Vector2, float> OnUsed;   // (center, radius)
        public event Action OnReady;

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= Time.deltaTime;
                if (CooldownRemaining <= 0f) OnReady?.Invoke();
            }

            // Only fire when there is something to hit, so it isn't wasted between waves.
            if (IsReady && Enemy.AliveCount > 0 && InputReader.AbilityPressed()) Use();
        }

        private void Use()
        {
            foreach (var hit in Physics2D.OverlapCircleAll(center.position, Radius))
            {
                if (hit.TryGetComponent(out Enemy enemy)) enemy.TakeDamage(damage);
                else if (hit.TryGetComponent(out EnemyProjectile projectile)) projectile.Shoot(damage);
            }

            CooldownRemaining = Cooldown;
            OnUsed?.Invoke(center.position, Radius);
        }
    }
}
