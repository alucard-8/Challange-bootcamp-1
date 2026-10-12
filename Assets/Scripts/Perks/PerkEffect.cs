using Soulbreak.Economy;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Everything a perk effect is allowed to touch, passed in so effects stay plain data assets.
    public class PerkContext
    {
        public PlayerStats Stats;
        public Reactor.Reactor Reactor;
        public SoulWallet Wallet;
        public Weapons.WeaponRing Weapons;
    }

    // Strategy pattern: each concrete effect is one interchangeable "how to apply" strategy.
    // The shop just calls Apply() and never switches on perk types.
    public abstract class PerkEffect : ScriptableObject
    {
        public abstract void Apply(PerkContext context);
    }
}
