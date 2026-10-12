using Soulbreak.GameFlow;
using UnityEngine;
using UnityEngine.Rendering;

namespace Soulbreak.Fx
{
    // Blends in the desaturated "frozen" post-processing while paused, and slowly on game over.
    public class PauseEffect : MonoBehaviour
    {
        [SerializeField] private PauseController pause;
        [SerializeField] private GameManager game;
        [SerializeField] private Volume volume;

        private float gameOver;

        private void Update()
        {
            if (game != null && game.State == GameState.GameOver)
                gameOver = Mathf.MoveTowards(gameOver, 1f, Time.unscaledDeltaTime * 0.6f);
            volume.weight = Mathf.Max(pause.Blend, gameOver);
        }
    }
}
