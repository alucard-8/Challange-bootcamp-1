using Soulbreak.Economy;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    public class PerkContext
    {
        public PlayerStats Stats;
        public Reactor.Reactor Reactor;
        public SoulWallet Wallet;
        public Weapons.WeaponRing Weapons;
    }

    public abstract class PerkEffect : ScriptableObject
    {
        public abstract void Apply(PerkContext context);
    }
}
