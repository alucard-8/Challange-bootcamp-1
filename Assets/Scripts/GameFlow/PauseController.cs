using System;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    // Owns "is the game paused". Pausing eases time down to a stop (slow-mo into freeze) and back.
    // Views (pause menu, post-processing) read Blend / listen to OnPauseChanged — they never touch time.
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private float transition = 0.35f;

        public bool IsPaused { get; private set; }
        public float Blend { get; private set; }   // 0 = playing, 1 = fully paused (eased)

        public event Action<bool> OnPauseChanged;

        private float raw;

        private void Update()
        {
            bool canPause = game.State != GameState.GameOver && !SceneFader.IsBusy;
            if (canPause && InputReader.PausePressed()) SetPaused(!IsPaused);

            float goal = IsPaused ? 1f : 0f;
            if (Mathf.Approximately(raw, goal)) return;

            raw = Mathf.MoveTowards(raw, goal, Time.unscaledDeltaTime / transition);
            Blend = raw * raw * (3f - 2f * raw);
            if (game.State != GameState.GameOver) Time.timeScale = 1f - Blend;
        }

        public void SetPaused(bool paused)
        {
            if (paused == IsPaused) return;
            if (paused && game.State == GameState.GameOver) return;
            IsPaused = paused;
            OnPauseChanged?.Invoke(paused);
        }
    }
}
