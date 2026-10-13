using UnityEngine;

namespace Soulbreak.Enemies
{
    public static class EnemyFactory
    {
        public static Enemy Create(EnemyConfig config, Vector2 position, Transform target, EnemyChannels channels,
                                   float healthMultiplier = 1f, float speedMultiplier = 1f,
                                   EnemyModifier modifiers = EnemyModifier.None)
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
            enemy.Init(config, target, channels, healthMultiplier, speedMultiplier, modifiers);
            go.AddComponent<EnemyView>();

            if (config.stopDistance > 0f) go.AddComponent<EnemyRangedAttack>();
            if ((modifiers & EnemyModifier.Blink) != 0) go.AddComponent<EnemyBlink>();
            if (config.isBoss) go.AddComponent<BossBrain>();
            if (config.gravityPull > 0f) go.AddComponent<GravityWell>();
            return enemy;
        }
    }
}
