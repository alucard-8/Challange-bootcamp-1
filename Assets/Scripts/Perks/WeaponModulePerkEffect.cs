using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Perks
{
    // Installs weapon modules into random slots of the weapon ring.
    [CreateAssetMenu(menuName = "Soulbreak/Perk Effects/Weapon Module", fileName = "WeaponModuleEffect")]
    public class WeaponModulePerkEffect : PerkEffect
    {
        public ShotPattern module;
        [Min(1)] public int slots = 1;

        public override void Apply(PerkContext context) => context.Weapons.AddModule(module, slots);
    }
}
