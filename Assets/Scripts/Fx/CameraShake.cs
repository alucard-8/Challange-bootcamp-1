using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Fx
{
    // Observer: adds "trauma" on impactful events; trauma decays and shakes the camera.
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private float maxOffset = 0.35f;
        [SerializeField] private float decay = 1.8f;

        private Vector3 basePos;
        private float trauma;

        private void Awake() => basePos = transform.localPosition;

        private void OnEnable()
        {
            reactor.OnHit += HandleHit;
            nova.OnUsed += HandleNova;
            enemyKilled.OnRaised += HandleKill;
        }

        private void OnDisable()
        {
            reactor.OnHit -= HandleHit;
            nova.OnUsed -= HandleNova;
            enemyKilled.OnRaised -= HandleKill;
        }

        private void HandleHit(bool shielded) => AddTrauma(shielded ? 0.35f : 0.6f);
        private void HandleNova(Vector2 c, float r) => AddTrauma(0.5f);
        private void HandleKill(Enemy e) => AddTrauma(e.Config.isBoss ? 0.8f : 0.06f);

        public void AddTrauma(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        private void LateUpdate()
        {
            if (trauma <= 0f) { transform.localPosition = basePos; return; }

            float shake = trauma * trauma;
            float t = Time.unscaledTime * 25f;
            var offset = new Vector3(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f, 0f) * 2f;
            transform.localPosition = basePos + offset * maxOffset * shake;
            trauma = Mathf.Max(0f, trauma - decay * Time.unscaledDeltaTime);
        }
    }
}
