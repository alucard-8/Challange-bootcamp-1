using System;

using UnityEngine;

namespace Soulbreak.GameFlow
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private TimeScaleService time;
        [SerializeField] private float transition = 0.35f;

        public bool IsPaused { get; private set; }
        public float Blend { get; private set; }

        public event Action<bool> OnPauseChanged;

        private float raw;

        private void Update()
        {
            float goal = IsPaused ? 1f : 0f;
            if (Mathf.Approximately(raw, goal)) return;

            raw = Mathf.MoveTowards(raw, goal, Time.unscaledDeltaTime / transition);
            Blend = raw * raw * (3f - 2f * raw);
            if (Blend <= 0f) time.Clear("pause"); else time.Set("pause", 1f - Blend);
        }

        public void Toggle() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;
            if (paused && (game.State == GameState.GameOver || SceneFader.IsBusy)) return;
            IsPaused = paused;
            OnPauseChanged?.Invoke(paused);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) AutoPause();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) AutoPause();
        }

        private void AutoPause()
        {
            if (!Core.GameSettings.PauseOnUnfocus || IsPaused) return;
            if (game == null || game.State != GameState.Combat) return;
            SetPaused(true);
        }
    }
}
