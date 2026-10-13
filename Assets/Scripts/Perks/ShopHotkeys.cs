using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Perks
{
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
