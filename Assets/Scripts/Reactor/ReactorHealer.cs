using Soulbreak.Orbs;
using UnityEngine;

namespace Soulbreak.Reactor
{
    // Observer: when a heal orb is collected, restore one shield charge.
    public class ReactorHealer : MonoBehaviour
    {
        [SerializeField] private Reactor reactor;
        [SerializeField] private OrbEventChannel orbCollected;

        private void OnEnable() => orbCollected.OnRaised += HandleOrbCollected;
        private void OnDisable() => orbCollected.OnRaised -= HandleOrbCollected;

        private void HandleOrbCollected(Orb orb)
        {
            if (orb.Type == OrbType.Heal) reactor.RestoreShieldCharge();
        }
    }
}
