using UnityEngine;

namespace Soulbreak.Enemies
{
    // One asset per enemy type. New enemy = new asset, no new code (Open/Closed).
    [CreateAssetMenu(menuName = "Soulbreak/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Visuals")]
        public Sprite sprite;
        public Color color = Color.red;
        [Min(0.1f)] public float size = 0.8f;

        [Header("Stats")]
        [Min(1)] public int maxHealth = 1;
        [Min(0.1f)] public float speed = 1.5f;
        [Min(1)] public int damage = 1;

        [Header("Rewards")]
        [Min(0)] public int soulReward = 5;
        [Range(0, 1)] public float soulOrbChance = 0.3f;
        [Min(0)] public int soulOrbValue = 15;
        [Range(0, 1)] public float healOrbChance = 0.05f;
    }
}
