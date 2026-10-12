using Soulbreak.Orbs;
using UnityEngine;

namespace Soulbreak.Reactor
{
    // Observer: when a heal orb is collected, restore one shield charge.
    public class ReactorHealer : MonoBehaviour
    {
        [SerializeField] private Reactor reactor;

        private void OnEnable() => Orb.OnAnyCollected += HandleOrbCollected;
        private void OnDisable() => Orb.OnAnyCollected -= HandleOrbCollected;

        private void HandleOrbCollected(Orb orb)
        {
            if (orb.Type == OrbType.Heal) reactor.RestoreShieldCharge();
        }
    }
}
