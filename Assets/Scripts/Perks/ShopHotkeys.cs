using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Input layer for the shop: number keys buy the matching offer. The view only displays.
    public class ShopHotkeys : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;

        private void Update()
        {
            if (!shop.IsOpen || Time.timeScale == 0f) return;
            for (int i = 0; i < shop.Offers.Count; i++)
                if (InputReader.PerkPressed(i)) shop.TryBuy(i);
        }
    }
}
