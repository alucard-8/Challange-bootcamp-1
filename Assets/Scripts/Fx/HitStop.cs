using System;
using Soulbreak.Core.Events;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.Fx
{
    public class HitStop : MonoBehaviour
    {
        [SerializeField] private TimeScaleService time;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private Vector2EventChannel headshots;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private EnemyEventChannel bossEnraged;

        public event Action<float> OnImpact;

        private float remaining;

        private void OnEnable()
        {
            headshots.OnRaised += HandleHeadshot;
            enemyKilled.OnRaised += HandleKill;
            bossEnraged.OnRaised += HandleEnrage;
            reactor.OnHit += HandleReactorHit;
        }

        private void OnDisable()
        {
            headshots.OnRaised -= HandleHeadshot;
            enemyKilled.OnRaised -= HandleKill;
            bossEnraged.OnRaised -= HandleEnrage;
            reactor.OnHit -= HandleReactorHit;
            time.Clear("hitstop");
        }

        private void HandleHeadshot(Vector2 p) => Stop(0.04f, 0.35f);
        private void HandleEnrage(Enemy e) => Stop(0.25f, 1f);
        private void HandleReactorHit(bool shielded) => Stop(shielded ? 0.05f : 0.09f, shielded ? 0.3f : 0.7f);

        private void HandleKill(Enemy e)
        {
            if (e.Config.isBoss) Stop(0.3f, 1f);
            else if (e.IsElite || e.Config.size >= 1.6f) Stop(0.05f, 0.4f);
        }

        private void Stop(float duration, float strength)
        {
            remaining = Mathf.Max(remaining, duration);
            time.Set("hitstop", 0.03f);
            OnImpact?.Invoke(strength);
        }

        private void Update()
        {
            if (remaining <= 0f) return;
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0f) time.Clear("hitstop");
        }
    }
}
