using System;

namespace Soulbreak.Enemies
{
    [Serializable]
    public class EnemyChannels
    {
        public EnemyEventChannel spawned;
        public EnemyEventChannel killed;
        public EnemyEventChannel reachedTarget;
        public EnemyProjectileEventChannel projectileShotDown;
        public EnemyEventChannel bossEnraged;
        public EnemyDamageEventChannel damaged;
    }
}
