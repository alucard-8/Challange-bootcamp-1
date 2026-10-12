using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Player
{
    // Panic button: blasts every hostile near the reactor, then goes on cooldown.
    // Triggered by PlayerController; targets IHostile only.
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
            if (CooldownRemaining <= 0f) return;
            CooldownRemaining -= Time.deltaTime;
            if (CooldownRemaining <= 0f) OnReady?.Invoke();
        }

        // Tutorial / cheats: make it ready right now.
        public void ResetCooldown()
        {
            if (CooldownRemaining <= 0f) return;
            CooldownRemaining = 0f;
            OnReady?.Invoke();
        }

        // Only fires if something is in range, so it isn't wasted between waves.
        public bool TryUse()
        {
            if (!IsReady || Time.timeScale == 0f) return false;

            var hits = Physics2D.OverlapCircleAll(center.position, Radius);
            bool any = false;
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out IHostile h) || !h.IsAlive) continue;
                h.Shoot(damage);
                any = true;
            }
            if (!any) return false;

            CooldownRemaining = Cooldown;
            OnUsed?.Invoke(center.position, Radius);
            return true;
        }
    }
}
