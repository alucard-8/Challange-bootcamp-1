using UnityEngine;

namespace Soulbreak.Weapons
{
    // A technique combo: owning BOTH modules makes firing the trigger module also fire a bonus pattern.
    [CreateAssetMenu(menuName = "Soulbreak/Synergy", fileName = "Synergy")]
    public class SynergyDefinition : ScriptableObject
    {
        public string displayName = "Synergy";
        [TextArea] public string description;
        public ShotPattern moduleA;
        public ShotPattern moduleB;
        [Tooltip("Which of the two modules triggers the bonus when fired")]
        public ShotPattern trigger;
        public ShotPattern bonus;
        [Min(0.1f)] public float bonusDamageMultiplier = 1f;

        public bool IsActive(WeaponRing ring) => ring.Owns(moduleA) && ring.Owns(moduleB);
        public bool Involves(ShotPattern p) => p == moduleA || p == moduleB;
        public ShotPattern Partner(ShotPattern p) => p == moduleA ? moduleB : moduleA;
    }
}
