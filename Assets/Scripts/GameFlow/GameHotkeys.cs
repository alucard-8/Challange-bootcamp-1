using Soulbreak.Core;
using Soulbreak.UI;
using UnityEngine;

namespace Soulbreak.GameFlow
{
    public class GameHotkeys : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private PauseController pause;
        [SerializeField] private PauseMenuView pauseMenu;

        private void Update()
        {
            if (InputReader.PausePressed())
            {
                if (pauseMenu == null || !pause.IsPaused || !pauseMenu.CloseTopPanel()) pause.Toggle();
            }
            if (game.State == GameState.GameOver && InputReader.RestartPressed()) game.Restart();
        }
    }
}
