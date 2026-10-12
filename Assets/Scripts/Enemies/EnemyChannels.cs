using System;

namespace Soulbreak.Enemies
{
    // The channels an enemy broadcasts on. Handed to every enemy by whoever spawns it (WaveManager,
    // EnemySplitter), so runtime-spawned enemies can raise events without any static/global state.
    [Serializable]
    public class EnemyChannels
    {
        public EnemyEventChannel spawned;
        public EnemyEventChannel killed;
        public EnemyEventChannel reachedTarget;
        public EnemyProjectileEventChannel projectileShotDown;
        public EnemyEventChannel bossEnraged;
    }
}
