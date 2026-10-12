using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    // Input layer for game-level commands: pause/resume and restart after game over.
    public class GameHotkeys : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private PauseController pause;

        private void Update()
        {
            if (InputReader.PausePressed()) pause.Toggle();
            if (game.State == GameState.GameOver && InputReader.RestartPressed()) game.Restart();
        }
    }
}
