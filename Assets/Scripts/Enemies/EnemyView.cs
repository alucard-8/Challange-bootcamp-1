using UnityEngine;

namespace Soulbreak.Enemies
{
    // Visual side of an enemy: feeds the enemy shader (color, spikes, hit flash, health ring,
    // elite aura, shield bubble). Split from Enemy so gameplay and visuals change independently (SRP).
    [RequireComponent(typeof(Enemy), typeof(SpriteRenderer))]
    public class EnemyView : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int HealthId = Shader.PropertyToID("_HealthFrac");
        private static readonly int SpikesId = Shader.PropertyToID("_Spikes");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int AuraId = Shader.PropertyToID("_AuraColor");
        private static readonly int ShieldId = Shader.PropertyToID("_ShieldBubble");

        private Enemy enemy;
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;
        private Vector3 baseScale;
        private float flash, appear;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            sr = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
            baseScale = transform.localScale;   // set by Enemy.Init (elites are a bit bigger)
            transform.localScale = Vector3.zero;
        }

        private void OnEnable() => enemy.OnDamaged += HandleDamaged;
        private void OnDisable() => enemy.OnDamaged -= HandleDamaged;

        private void Start()
        {
            sr.sprite = enemy.Config.sprite;
            if (enemy.Config.material != null) sr.sharedMaterial = enemy.Config.material;
            sr.sortingOrder = enemy.Config.isBoss ? 3 : 2;
            sr.GetPropertyBlock(block);
            block.SetColor(ColorId, enemy.Config.color);
            block.SetFloat(SpikesId, enemy.Config.spikes);
            block.SetFloat(SeedId, Random.value * 10f);
            block.SetColor(AuraId, EnemyModifierInfo.AuraColor(enemy.Modifiers));
            Apply();
        }

        private void HandleDamaged(Enemy e) => flash = 1f;

        private void Update()
        {
            // Spawn-in: grow from nothing with a small overshoot.
            if (appear < 1f)
            {
                appear = Mathf.Min(1f, appear + Time.deltaTime * 3f);
                float s = 1f + 2.2f * Mathf.Pow(appear - 1f, 3f) + 1.2f * Mathf.Pow(appear - 1f, 2f);
                transform.localScale = baseScale * s;
            }

            if (flash <= 0f) return;
            flash = Mathf.Max(0f, flash - Time.deltaTime * 10f);
            Apply();
        }

        private void Apply()
        {
            block.SetFloat(FlashId, flash);
            block.SetFloat(HealthId, (float)enemy.Health / enemy.MaxHealth);
            block.SetFloat(ShieldId, enemy.ShieldHits / 3f);
            sr.SetPropertyBlock(block);
        }
    }
}
