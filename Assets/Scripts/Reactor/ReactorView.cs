using UnityEngine;

namespace Soulbreak.Reactor
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ReactorView : MonoBehaviour
    {
        private static readonly int HealthId = Shader.PropertyToID("_Health01");
        private static readonly int ShieldId = Shader.PropertyToID("_Shield");
        private static readonly int ShieldMaxId = Shader.PropertyToID("_ShieldMax");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int CracksId = Shader.PropertyToID("_Cracks");
        private static readonly int AlarmId = Shader.PropertyToID("_Alarm");
        private static readonly int CoreColorId = Shader.PropertyToID("_Color");
        private static readonly int ShieldColorId = Shader.PropertyToID("_ShieldColor");

        private static readonly int UpOffenseId = Shader.PropertyToID("_UpOffense");
        private static readonly int UpDefenseId = Shader.PropertyToID("_UpDefense");
        private static readonly int UpEconomyId = Shader.PropertyToID("_UpEconomy");
        private static readonly int TierId = Shader.PropertyToID("_Tier");
        private static readonly int TierPulseId = Shader.PropertyToID("_TierPulse");

        public void SetUpgrades(float offense, float defense, float economy, int tier, float tierPulse)
        {
            block.SetFloat(UpOffenseId, offense);
            block.SetFloat(UpDefenseId, defense);
            block.SetFloat(UpEconomyId, economy);
            block.SetFloat(TierId, tier);
            block.SetFloat(TierPulseId, tierPulse);
            Apply();
        }

        public void SetColors(Color core, Color shield)
        {
            block.SetColor(CoreColorId, core);
            block.SetColor(ShieldColorId, shield);
            Apply();
        }

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

        private void HandleHealth(int cur, int max)
        {
            float h = max > 0 ? (float)cur / max : 0f;
            block.SetFloat(HealthId, h);
            block.SetFloat(CracksId, 1f - h);
            block.SetFloat(AlarmId, h <= 0.3f && cur > 0 ? 1f : 0f);
            Apply();
        }
        private void HandleShield(int cur, int max)
        {
            block.SetFloat(ShieldId, reactor.ShieldSuppressed ? 0 : cur);
            block.SetFloat(ShieldMaxId, max);
            Apply();
        }
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
