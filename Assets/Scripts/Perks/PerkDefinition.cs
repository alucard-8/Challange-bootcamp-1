using UnityEngine;

namespace Soulbreak.Perks
{
    public enum PerkCategory { Offense, Economy, Defense, Ability, Weapon }
    public enum PerkRarity { Common, Rare, Epic, Legendary }

    [CreateAssetMenu(menuName = "Soulbreak/Perk", fileName = "Perk")]
    public class PerkDefinition : ScriptableObject
    {
        public string displayName = "Perk";
        [TextArea] public string description;
        public PerkCategory category;
        public PerkRarity rarity;
        [Min(0)] public int baseCost = 50;
        [Min(0)] public int costGrowth = 25;
        [Tooltip("0 = unlimited")]
        [Min(0)] public int maxStacks = 0;
        public PerkEffect[] effects;

        public WeaponModulePerkEffect TechniqueEffect
        {
            get
            {
                foreach (var e in effects)
                    if (e is WeaponModulePerkEffect w) return w;
                return null;
            }
        }

        public bool IsTechnique => TechniqueEffect != null;

        public void Apply(PerkContext context)
        {
            foreach (var effect in effects)
                if (effect != null) effect.Apply(context);
        }
    }
}
