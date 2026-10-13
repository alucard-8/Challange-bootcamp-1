using System;
using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using UnityEngine;

namespace Soulbreak.Player
{
    public class OverdriveSystem : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private TimeScaleService time;
        [SerializeField] private float chargePerKill = 3f;
        [SerializeField] private float duration = 8f;
        [SerializeField] private float timeScale = 0.8f;

        private const float Max = 100f;

        public float Meter { get; private set; }
        public float Meter01 => Meter / Max;
        public bool IsReady => !IsActive && Meter >= Max;
        public bool IsActive { get; private set; }
        public float Remaining { get; private set; }

        public event Action OnReady;
        public event Action OnStarted;
        public event Action OnEnded;

        private void OnEnable() => enemyKilled.OnRaised += HandleKill;
        private void OnDisable()
        {
            enemyKilled.OnRaised -= HandleKill;
            time.Clear("overdrive");
        }

        private void HandleKill(Enemy e)
        {
            if (IsActive || Meter >= Max) return;
            float gain = chargePerKill * (e.Config.isBoss ? 15f : e.IsElite ? 3f : 1f);
            Meter = Mathf.Min(Max, Meter + gain);
            if (Meter >= Max) OnReady?.Invoke();
        }

        public void Fill()
        {
            if (IsActive || Meter >= Max) return;
            Meter = Max;
            OnReady?.Invoke();
        }

        public bool TryActivate()
        {
            if (!IsReady || Time.timeScale == 0f) return false;
            IsActive = true;
            Remaining = duration;
            time.Set("overdrive", timeScale);
            OnStarted?.Invoke();
            return true;
        }

        private void Update()
        {
            if (!IsActive) return;
            Remaining -= Time.unscaledDeltaTime;
            Meter = Max * Mathf.Clamp01(Remaining / duration);
            if (Remaining > 0f) return;

            IsActive = false;
            Meter = 0f;
            time.Clear("overdrive");
            OnEnded?.Invoke();
        }
    }
}
