using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Menu
{
    // Input layer for the main menu: Enter plays (via the difficulty picker), Esc closes open panels,
    // mouse drives the parallax and clicks feed the core easter egg.
    public class MainMenuInput : MonoBehaviour
    {
        [SerializeField] private MainMenuView view;
        [SerializeField] private MenuAmbience ambience;
        [SerializeField] private ReactorEasterEgg easterEgg;
        [SerializeField] private Meta.MetaCatalog catalog;
        [SerializeField] private Meta.CodexCatalog codex;
        [SerializeField] private IndexView index;
        [SerializeField] private UI.AccountView account;

        private void Update()
        {
            // Admin (editor / dev builds only): F1 unlocks every skin, technique and mode.
            if (InputReader.AdminPressed() && catalog != null && (Application.isEditor || Debug.isDebugBuild))
            {
                Meta.MetaProgress.UnlockAll(catalog);
                if (codex != null) Meta.MetaProgress.DiscoverAll(codex);
                Debug.Log("ADMIN: unlocked everything.");
            }

            ambience.LookAt(InputReader.PointerPosition());

            bool anyPanel = view.IsAscensionOpen || view.IsHowToOpen || view.IsSettingsOpen || view.IsDifficultyOpen || index.IsOpen || account.IsOpen;
            if (InputReader.FirePressed() && !anyPanel && Camera.main != null)
                easterEgg.RegisterClick(Camera.main.ScreenToWorldPoint(InputReader.PointerPosition()));

            bool esc = InputReader.PausePressed();
            bool enter = InputReader.ConfirmPressed();

            if (account.IsOpen) { if (esc) account.Close(); }   // typing: Enter must not start a run
            else if (index.IsOpen)
            {
                if (esc) index.Close();
                else if (InputReader.LeftPressed()) index.TurnPage(-1);
                else if (InputReader.RightPressed()) index.TurnPage(1);
            }
            else if (view.IsSettingsOpen) { if (esc) view.CloseSettings(); }
            else if (view.IsDifficultyOpen)
            {
                if (esc) view.CloseDifficulty();
                else if (enter) view.ConfirmDifficulty();
            }
            else if (view.IsAscensionOpen) { if (esc) view.CloseAscension(); }
            else if (view.IsHowToOpen) { if (esc) view.CloseHowTo(); }
            else if (enter) view.OpenPlay();
        }
    }
}
