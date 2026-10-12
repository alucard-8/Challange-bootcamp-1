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
        [SerializeField] private float radius = 4f;
        [SerializeField] private int damage = 99;
        [SerializeField] private float cooldown = 12f;

        public float CooldownRemaining { get; private set; }
        public float Cooldown => cooldown;
        public bool IsReady => CooldownRemaining <= 0f;

        public event Action OnUsed;
        public event Action OnReady;

        private void Update()
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining -= Time.deltaTime;
                if (CooldownRemaining <= 0f) OnReady?.Invoke();
            }

            if (Time.timeScale > 0f && IsReady && InputReader.AbilityPressed()) Use();
        }

        private void Use()
        {
            foreach (var hit in Physics2D.OverlapCircleAll(center.position, radius))
                if (hit.TryGetComponent(out Enemy enemy)) enemy.TakeDamage(damage);

            CooldownRemaining = cooldown;
            OnUsed?.Invoke();
        }
    }
}
