using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Weapons
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Beam Event Channel", fileName = "BeamEventChannel")]
    public class BeamEventChannel : EventChannel<BeamInfo> { }
}
