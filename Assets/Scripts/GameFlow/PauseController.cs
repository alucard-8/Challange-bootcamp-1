using System;

using UnityEngine;

namespace Soulbreak.GameFlow
{
    // Owns "is the game paused". Pausing eases time down to a stop (slow-mo into freeze) and back.
    // Views (pause menu, post-processing) read Blend / listen to OnPauseChanged — they never touch time.
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private TimeScaleService time;
        [SerializeField] private float transition = 0.35f;

        public bool IsPaused { get; private set; }
        public float Blend { get; private set; }   // 0 = playing, 1 = fully paused (eased)

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
    }
}
