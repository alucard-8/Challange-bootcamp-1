using UnityEngine;

namespace Soulbreak.Perks
{
    public enum PerkType { Damage, SoulGain, ShieldRestore }

    [CreateAssetMenu(menuName = "Soulbreak/Perk Config", fileName = "PerkConfig")]
    public class PerkConfig : ScriptableObject
    {
        public string displayName = "Perk";
        public PerkType type;
        public float amount = 1f;
        [Min(0)] public int baseCost = 50;
        [Min(0)] public int costIncreasePerBuy = 25;
    }
}
