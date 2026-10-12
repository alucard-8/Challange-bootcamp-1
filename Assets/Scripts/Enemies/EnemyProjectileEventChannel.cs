using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Enemy Projectile Event Channel", fileName = "EnemyProjectileEventChannel")]
    public class EnemyProjectileEventChannel : EventChannel<EnemyProjectile> { }
}
