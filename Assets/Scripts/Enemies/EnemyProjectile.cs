using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    // Enemy bullet flying at the reactor. The player can shoot it down (it's IHostile/IShootable).
    public class EnemyProjectile : MonoBehaviour, IHostile
    {
        private Transform target;
        private float speed;
        private int damage;
        private EnemyProjectileEventChannel shotDownChannel;

        public bool IsAlive => this != null;
        public bool IsHeavy => false;

        public static EnemyProjectile Spawn(Vector2 position, Transform target, EnemyConfig source, EnemyProjectileEventChannel shotDown)
        {
            var go = new GameObject("EnemyProjectile");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.45f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = source.sprite;
            if (source.projectileMaterial != null) sr.sharedMaterial = source.projectileMaterial;
            sr.color = source.color;
            sr.sortingOrder = 4;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var p = go.AddComponent<EnemyProjectile>();
            p.target = target;
            p.speed = source.projectileSpeed;
            p.damage = source.projectileDamage;
            p.shotDownChannel = shotDown;
            return p;
        }

        private void Update()
        {
            if (target == null) { Destroy(gameObject); return; }
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        public void Shoot(int dmg)
        {
            if (shotDownChannel != null) shotDownChannel.Raise(this);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform != target) return;
            if (other.TryGetComponent(out IDamageable d)) d.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
