using Soulbreak.Core;
using Soulbreak.Economy;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Testing helper: F1 grants every perk/module and a pile of souls.
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

        private void Update()
        {
            if (!InputReader.AdminPressed()) return;
            shop.GrantAll();
            wallet.Add(souls, transform.position);
            Debug.Log("ADMIN: granted every perk and " + souls + " souls.");
        }
    }
}
