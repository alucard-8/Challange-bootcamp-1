using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Enemies
{
    public struct EnemyDamage
    {
        public Enemy Enemy;
        public Vector2 Position;
        public int Amount;
        public bool Blocked;
        public bool Killed;
    }

    [CreateAssetMenu(menuName = "Soulbreak/Events/Enemy Damage Event Channel", fileName = "EnemyDamageEventChannel")]
    public class EnemyDamageEventChannel : EventChannel<EnemyDamage> { }
}
