using Soulbreak.Core;
using Soulbreak.Economy;
using UnityEngine;

namespace Soulbreak.Perks
{
    public class AdminCheats : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private int souls = 9999;

        private void Awake()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) enabled = false;
        }

        [SerializeField] private Reactor.Reactor reactor;

        [SerializeField] private UI.GameOverView gameOverView;

        private void Update()
        {
            if (InputReader.PreviewGameOverPressed() && gameOverView != null) gameOverView.PreviewNewBest();

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
