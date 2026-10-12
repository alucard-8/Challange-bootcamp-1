using UnityEngine;

namespace Soulbreak.Reactor
{
    // Data only — tweak values in the Inspector without touching code (Open/Closed).
    [CreateAssetMenu(menuName = "Soulbreak/Reactor Config", fileName = "ReactorConfig")]
    public class ReactorConfig : ScriptableObject
    {
        [Min(1)] public int maxHealth = 10;
        [Min(0)] public int maxShieldCharges = 3;
    }
}
