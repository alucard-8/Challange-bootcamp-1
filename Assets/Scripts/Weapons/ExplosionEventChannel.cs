using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Weapons
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Explosion Event Channel", fileName = "ExplosionEventChannel")]
    public class ExplosionEventChannel : EventChannel<ExplosionInfo> { }
}
