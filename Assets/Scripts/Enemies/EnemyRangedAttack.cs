using UnityEngine;

namespace Soulbreak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyRangedAttack : MonoBehaviour
    {
        private Enemy enemy;
        private float timer;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            timer = 0.6f;
        }

        private void Update()
        {
            if (!enemy.InRange || enemy.Target == null) return;

            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = enemy.Config.fireInterval;

            EnemyProjectile.Spawn(transform.position, enemy.Target, enemy.Config, enemy.Channels?.projectileShotDown);
        }
    }
}
