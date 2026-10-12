using System;
using Soulbreak.Core;
using Soulbreak.Economy;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Spends souls on perks (keys 1..N). Applies the effect, then announces it.
    public class PerkShop : MonoBehaviour
    {
        [SerializeField] private PerkConfig[] perks;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Reactor.Reactor reactor;

        private int[] timesBought;

        public int PerkCount => perks.Length;
        public PerkConfig GetPerk(int i) => perks[i];
        public int GetCost(int i) => perks[i].baseCost + perks[i].costIncreasePerBuy * timesBought[i];

        public event Action<PerkConfig> OnPerkBought;
        public event Action<PerkConfig> OnPurchaseFailed;

        private void Awake() => timesBought = new int[perks.Length];

        private void Update()
        {
            if (Time.timeScale == 0f) return;
            for (int i = 0; i < perks.Length; i++)
                if (InputReader.PerkPressed(i)) TryBuy(i);
        }

        public bool TryBuy(int i)
        {
            var perk = perks[i];
            if (!wallet.TrySpend(GetCost(i)))
            {
                OnPurchaseFailed?.Invoke(perk);
                return false;
            }

            Apply(perk);
            timesBought[i]++;
            OnPerkBought?.Invoke(perk);
            return true;
        }

        private void Apply(PerkConfig perk)
        {
            switch (perk.type)
            {
                case PerkType.Damage: stats.AddDamage(Mathf.RoundToInt(perk.amount)); break;
                case PerkType.SoulGain: stats.AddSoulMultiplier(perk.amount); break;
                case PerkType.ShieldRestore: reactor.RestoreShieldCharge(); break;
            }
        }
    }
}
