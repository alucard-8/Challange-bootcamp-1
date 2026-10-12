using UnityEngine;

namespace Soulbreak.Enemies
{
    // Factory pattern: the only place that knows how an enemy GameObject is assembled.
    public static class EnemyFactory
    {
        public static Enemy Create(EnemyConfig config, Vector2 position, Transform target, EnemyChannels channels,
                                   float healthMultiplier = 1f, float speedMultiplier = 1f)
        {
            var go = new GameObject(config.name);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = config.sprite;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var enemy = go.AddComponent<Enemy>();
            enemy.Init(config, target, channels, healthMultiplier, speedMultiplier);
            go.AddComponent<EnemyView>();

            if (config.stopDistance > 0f) go.AddComponent<EnemyRangedAttack>();
            return enemy;
        }
    }
}
