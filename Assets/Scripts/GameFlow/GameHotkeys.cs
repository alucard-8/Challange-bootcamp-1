using Soulbreak.Core;
using Soulbreak.UI;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    // Input layer for game-level commands: pause/resume (Esc closes Settings first) and restart after game over.
    public class GameHotkeys : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private PauseController pause;
        [SerializeField] private PauseMenuView pauseMenu;

        private void Update()
        {
            if (InputReader.PausePressed())
            {
                // close Settings / Index / How to play first, then unpause
                if (pauseMenu == null || !pause.IsPaused || !pauseMenu.CloseTopPanel()) pause.Toggle();
            }
            if (game.State == GameState.GameOver && InputReader.RestartPressed()) game.Restart();
        }
    }
}
