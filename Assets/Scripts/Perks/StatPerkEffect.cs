using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Perks
{
    public enum StatOperation { Add, Multiply }

    // Modifies one player stat. Covers most perks with zero extra code.
    [CreateAssetMenu(menuName = "Soulbreak/Perk Effects/Stat", fileName = "StatEffect")]
    public class StatPerkEffect : PerkEffect
    {
        public StatType stat;
        public StatOperation operation = StatOperation.Add;
        public float value = 1f;

        public override void Apply(PerkContext context)
        {
            if (operation == StatOperation.Add) context.Stats.Add(stat, value);
            else context.Stats.Multiply(stat, value);
        }
    }
}
