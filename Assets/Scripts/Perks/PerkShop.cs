using System;
using System.Collections.Generic;
using Soulbreak.Economy;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    public class PerkOffer
    {
        public PerkDefinition Perk;
        public int Cost;
        public bool Sold;
    }

    // Shop logic only (the ShopView draws it). Rolls random offers weighted by rarity,
    // sells them for souls, rerolls, and announces everything through events.
    public class PerkShop : MonoBehaviour
    {
        [SerializeField] private PerkDefinition[] catalog;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private Weapons.WeaponRing weapons;

        [Header("Offers")]
        [SerializeField] private int offerCount = 5;
        [SerializeField] private int baseRerollCost = 15;
        [SerializeField] private int rerollCostGrowth = 10;
        [SerializeField] private float[] rarityWeights = { 60f, 28f, 10f, 2f };

        public bool IsOpen { get; private set; }
        public int RerollCost { get; private set; }
        public IReadOnlyList<PerkOffer> Offers => offers;
        public int GetStacks(PerkDefinition perk) => stacks.TryGetValue(perk, out int n) ? n : 0;

        public event Action OnOpened;
        public event Action OnClosed;
        public event Action OnOffersChanged;
        public event Action<PerkDefinition> OnPerkBought;
        public event Action OnPurchaseFailed;

        private readonly List<PerkOffer> offers = new List<PerkOffer>();
        private readonly Dictionary<PerkDefinition, int> stacks = new Dictionary<PerkDefinition, int>();
        private PerkContext context;

        private void Awake() => context = new PerkContext { Stats = stats, Reactor = reactor, Wallet = wallet, Weapons = weapons };

        public void Open()
        {
            IsOpen = true;
            RerollCost = baseRerollCost;
            Roll();
            OnOpened?.Invoke();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            OnClosed?.Invoke();
        }

        public bool TryBuy(int index)
        {
            if (!IsOpen || index < 0 || index >= offers.Count) return false;
            var offer = offers[index];
            if (offer.Sold || !wallet.TrySpend(offer.Cost))
            {
                OnPurchaseFailed?.Invoke();
                return false;
            }

            offer.Perk.Apply(context);
            stacks[offer.Perk] = GetStacks(offer.Perk) + 1;
            offer.Sold = true;
            OnPerkBought?.Invoke(offer.Perk);
            OnOffersChanged?.Invoke();
            return true;
        }

        public bool TryReroll()
        {
            if (!IsOpen || !wallet.TrySpend(RerollCost))
            {
                OnPurchaseFailed?.Invoke();
                return false;
            }
            RerollCost += rerollCostGrowth;
            Roll();
            return true;
        }

        // Debug/admin: apply every perk up to its max stacks (unlimited perks once), for free.
        public void GrantAll()
        {
            foreach (var perk in catalog)
            {
                if (perk == null) continue;
                int target = perk.maxStacks > 0 ? perk.maxStacks : GetStacks(perk) + 1;
                while (GetStacks(perk) < target)
                {
                    perk.Apply(context);
                    stacks[perk] = GetStacks(perk) + 1;
                    OnPerkBought?.Invoke(perk);
                }
            }
            if (IsOpen) Roll();
        }

        public int CostOf(PerkDefinition perk) => perk.baseCost + perk.costGrowth * GetStacks(perk);

        private bool IsMaxed(PerkDefinition perk) => perk.maxStacks > 0 && GetStacks(perk) >= perk.maxStacks;

        private void Roll()
        {
            offers.Clear();
            var candidates = new List<PerkDefinition>();
            foreach (var p in catalog)
                if (p != null && !IsMaxed(p)) candidates.Add(p);

            for (int i = 0; i < offerCount && candidates.Count > 0; i++)
            {
                var pick = PickWeighted(candidates);
                candidates.Remove(pick);
                offers.Add(new PerkOffer { Perk = pick, Cost = CostOf(pick) });
            }
            OnOffersChanged?.Invoke();
        }

        private PerkDefinition PickWeighted(List<PerkDefinition> list)
        {
            float total = 0f;
            foreach (var p in list) total += Weight(p);
            float roll = UnityEngine.Random.value * total;
            foreach (var p in list)
            {
                roll -= Weight(p);
                if (roll <= 0f) return p;
            }
            return list[list.Count - 1];
        }

        private float Weight(PerkDefinition p)
        {
            int r = (int)p.rarity;
            return r < rarityWeights.Length ? rarityWeights[r] : 1f;
        }
    }
}
