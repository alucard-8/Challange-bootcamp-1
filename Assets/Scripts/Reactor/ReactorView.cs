using UnityEngine;

namespace Soulbreak.Reactor
{
    // Visual side of the reactor: feeds health / shield / hit flash into the Reactor shader.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ReactorView : MonoBehaviour
    {
        private static readonly int HealthId = Shader.PropertyToID("_Health01");
        private static readonly int ShieldId = Shader.PropertyToID("_Shield");
        private static readonly int ShieldMaxId = Shader.PropertyToID("_ShieldMax");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");

        [SerializeField] private Reactor reactor;

        private SpriteRenderer sr;
        private MaterialPropertyBlock block;
        private float flash;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            reactor.OnHealthChanged += HandleHealth;
            reactor.OnShieldChanged += HandleShield;
            reactor.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            reactor.OnHealthChanged -= HandleHealth;
            reactor.OnShieldChanged -= HandleShield;
            reactor.OnHit -= HandleHit;
        }

        private void HandleHealth(int cur, int max) { block.SetFloat(HealthId, max > 0 ? (float)cur / max : 0f); Apply(); }
        private void HandleShield(int cur, int max) { block.SetFloat(ShieldId, cur); block.SetFloat(ShieldMaxId, max); Apply(); }
        private void HandleHit(bool shielded) => flash = 1f;

        private void Update()
        {
            if (flash <= 0f) return;
            flash = Mathf.Max(0f, flash - Time.unscaledDeltaTime * 4f);
            Apply();
        }

        private void Apply()
        {
            block.SetFloat(FlashId, flash);
            sr.SetPropertyBlock(block);
        }
    }
}
