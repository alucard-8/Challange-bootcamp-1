using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Weapons
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Chain Event Channel", fileName = "ChainEventChannel")]
    public class ChainEventChannel : EventChannel<ChainInfo> { }
}
