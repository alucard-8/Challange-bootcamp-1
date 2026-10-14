using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    public class EnemyProjectile : MonoBehaviour, IHostile
    {
        private static readonly Stack<EnemyProjectile> pool = new Stack<EnemyProjectile>();

        private SpriteRenderer sr;
        private Transform target;
        private float speed;
        private int damage;
        private EnemyProjectileEventChannel shotDownChannel;

        public bool IsAlive => this != null;
        public bool IsHeavy => false;

        public static EnemyProjectile Spawn(Vector2 position, Transform target, EnemyConfig source, EnemyProjectileEventChannel shotDown)
        {
            EnemyProjectile p = null;
            while (pool.Count > 0)
            {
                var candidate = pool.Pop();
                if (candidate != null) { p = candidate; break; }
            }

            if (p == null)
            {
                var go = new GameObject("EnemyProjectile");
                p = go.AddComponent<EnemyProjectile>();
                p.sr = go.AddComponent<SpriteRenderer>();
                p.sr.sortingOrder = 4;

                var col = go.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
                var rb = go.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else
            {
                p.gameObject.SetActive(true);
            }

            p.transform.position = position;
            p.transform.localScale = Vector3.one * 0.45f;
            p.sr.sprite = source.sprite;
            p.sr.sharedMaterial = source.projectileMaterial;
            p.sr.color = source.color;

            p.target = target;
            p.speed = source.projectileSpeed;
            p.damage = source.projectileDamage;
            p.shotDownChannel = shotDown;
            return p;
        }

        private void Despawn()
        {
            gameObject.SetActive(false);
            pool.Push(this);
        }

        private void Update()
        {
            if (target == null) { Despawn(); return; }
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }

        public void Shoot(int dmg)
        {
            if (shotDownChannel != null) shotDownChannel.Raise(this);
            Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform != target) return;
            if (other.TryGetComponent(out IDamageable d)) d.TakeDamage(damage);
            Despawn();
        }
    }
}
