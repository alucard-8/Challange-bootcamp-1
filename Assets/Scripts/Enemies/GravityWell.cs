using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Enemies
{
    [RequireComponent(typeof(Enemy))]
    public class GravityWell : MonoBehaviour
    {
        [SerializeField] private float radius = 7f;

        private Enemy enemy;

        private void Awake() => enemy = GetComponent<Enemy>();

        private void Update()
        {
            if (enemy.Target == null || enemy.Config.gravityPull <= 0f) return;
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, radius))
            {
                if (!hit.TryGetComponent(out IHostile h) || h.IsHeavy || !h.IsAlive) continue;
                h.transform.position = Vector3.MoveTowards(h.transform.position, enemy.Target.position, enemy.Config.gravityPull * Time.deltaTime);
            }
            transform.Rotate(0f, 0f, -40f * Time.deltaTime);
        }
    }
}
