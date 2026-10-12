using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Enemies
{
    public struct EnemyDamage
    {
        public Enemy Enemy;
        public Vector2 Position;
        public int Amount;
        public bool Blocked;   // absorbed by an elite shield
        public bool Killed;
    }

    // Raised every time an enemy is hit (damage numbers, hit sparks...).
    [CreateAssetMenu(menuName = "Soulbreak/Events/Enemy Damage Event Channel", fileName = "EnemyDamageEventChannel")]
    public class EnemyDamageEventChannel : EventChannel<EnemyDamage> { }
}
