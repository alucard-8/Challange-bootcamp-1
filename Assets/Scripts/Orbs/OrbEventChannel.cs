using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Orbs
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Orb Event Channel", fileName = "OrbEventChannel")]
    public class OrbEventChannel : EventChannel<Orb> { }
}
