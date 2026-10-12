using Soulbreak.Core;
using Soulbreak.Economy;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Testing helper: F1 grants every perk/module and souls, F2 destroys the core, F3 previews a NEW BEST game over.
    // Disabled in release builds — it only works in the Editor and Development builds.
    public class AdminCheats : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private int souls = 9999;

        private void Awake()
        {
            // Release builds: switch the cheat off entirely.
            if (!Application.isEditor && !Debug.isDebugBuild) enabled = false;
        }

        [SerializeField] private Reactor.Reactor reactor;

        [SerializeField] private UI.GameOverView gameOverView;

        private void Update()
        {
            // F3: preview the game-over screen with a NEW BEST (fake run, nothing saved)
            if (InputReader.PreviewGameOverPressed() && gameOverView != null) gameOverView.PreviewNewBest();

            // F2: destroy the core instantly (to test the game over screen)
            if (InputReader.KillSelfPressed() && reactor != null && !reactor.IsDestroyed)
            {
                while (!reactor.IsDestroyed) reactor.TakeDamage(999);
                Debug.Log("ADMIN: core destroyed.");
            }

            if (!InputReader.AdminPressed()) return;
            shop.GrantAll();
            wallet.Add(souls, transform.position);
            Debug.Log("ADMIN: granted every perk and " + souls + " souls.");
        }
    }
}
