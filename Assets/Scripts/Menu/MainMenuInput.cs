using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Menu
{
    // Input layer for the main menu: Enter plays, Esc closes How-to-play, mouse drives the parallax.
    public class MainMenuInput : MonoBehaviour
    {
        [SerializeField] private MainMenuView view;
        [SerializeField] private MenuAmbience ambience;

        private void Update()
        {
            ambience.LookAt(InputReader.PointerPosition());

            if (view.IsHowToOpen)
            {
                if (InputReader.PausePressed()) view.CloseHowTo();
            }
            else if (InputReader.ConfirmPressed()) view.Play();
        }
    }
}
