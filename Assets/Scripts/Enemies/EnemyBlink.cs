using UnityEngine;

namespace Soulbreak.Enemies
{
    // Elite "Blink" behaviour: every few seconds the enemy teleports a short hop toward its target.
    [RequireComponent(typeof(Enemy))]
    public class EnemyBlink : MonoBehaviour
    {
        [SerializeField] private float interval = 2.6f;
        [SerializeField] private float distance = 1.8f;

        private Enemy enemy;
        private float timer;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            timer = interval * Random.Range(0.6f, 1.2f);
        }

        private void Update()
        {
            if (enemy.Target == null) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = interval;

            Vector3 to = enemy.Target.position - transform.position;
            float hop = Mathf.Min(distance, Mathf.Max(0f, to.magnitude - 1.6f));   // never blink into the reactor
            transform.position += to.normalized * hop;
        }
    }
}
