using UnityEngine;

namespace Soulbreak.Enemies
{
    // Factory pattern: the only place that knows how an enemy GameObject is assembled.
    public static class EnemyFactory
    {
        public static Enemy Create(EnemyConfig config, Vector2 position, Transform target, float speedMultiplier)
        {
            var go = new GameObject(config.name);
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var enemy = go.AddComponent<Enemy>();
            enemy.Init(config, target, speedMultiplier);
            return enemy;
        }
    }
}
