using UnityEngine;

namespace Soulbreak.Perks
{
    public enum ReactorAction { Heal, AddMaxHealth, RestoreShield, AddMaxShield }

    // Repairs or upgrades the reactor.
    [CreateAssetMenu(menuName = "Soulbreak/Perk Effects/Reactor", fileName = "ReactorEffect")]
    public class ReactorPerkEffect : PerkEffect
    {
        public ReactorAction action;
        [Min(1)] public int amount = 1;

        public override void Apply(PerkContext context)
        {
            var r = context.Reactor;
            switch (action)
            {
                case ReactorAction.Heal: r.Heal(amount); break;
                case ReactorAction.AddMaxHealth: r.AddMaxHealth(amount); break;
                case ReactorAction.RestoreShield: r.RestoreShieldCharge(amount); break;
                case ReactorAction.AddMaxShield: r.AddMaxShield(amount); break;
            }
        }
    }
}
