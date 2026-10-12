using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Enemy Event Channel", fileName = "EnemyEventChannel")]
    public class EnemyEventChannel : EventChannel<Enemy> { }
}
