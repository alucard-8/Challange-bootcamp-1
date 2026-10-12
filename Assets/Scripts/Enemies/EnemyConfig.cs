using UnityEngine;

namespace Soulbreak.Enemies
{
    // One asset per enemy type. New enemy = new asset, no new code (Open/Closed).
    [CreateAssetMenu(menuName = "Soulbreak/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Visuals")]
        public Sprite sprite;
        public Material material;
        [ColorUsage(true, true)] public Color color = Color.red;
        [Min(0.1f)] public float size = 0.7f;
        [Tooltip("Number of spikes drawn by the enemy shader.")]
        [Min(0)] public float spikes = 6f;

        [Header("Stats")]
        [Min(1)] public int maxHealth = 1;
        [Min(0.1f)] public float speed = 1.5f;
        [Min(1)] public int damage = 1;
        public bool isBoss;
        [Tooltip("Boss only: minion type summoned in phase 1")]
        public EnemyConfig minion;
        public string displayName = "";

        [Header("Movement")]
        [Tooltip("Movement strategy. Empty = straight line toward the target.")]
        public MovementPattern movement;
        [Tooltip("Boss only: pulls other enemies toward the target (black hole)")]
        [Min(0)] public float gravityPull = 0f;

        [Header("Ranged (stopDistance > 0 makes it a shooter)")]
        [Min(0)] public float stopDistance = 0f;
        [Min(0.1f)] public float fireInterval = 2f;
        [Min(1)] public int projectileDamage = 1;
        [Min(0.1f)] public float projectileSpeed = 3f;
        public Material projectileMaterial;

        [Header("Splitting")]
        public EnemyConfig splitInto;
        [Min(0)] public int splitCount = 0;

        [Header("Rewards")]
        [Min(0)] public int soulReward = 5;
        [Range(0, 1)] public float soulOrbChance = 0.3f;
        [Min(0)] public int soulOrbValue = 15;
        [Range(0, 1)] public float healOrbChance = 0.05f;
    }
}
