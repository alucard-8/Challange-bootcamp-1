using UnityEngine;

namespace Soulbreak.Enemies
{
    // Visual side of an enemy: feeds the enemy shader (color, spikes, hit flash, health ring).
    // Split from Enemy so gameplay and visuals change independently (SRP).
    [RequireComponent(typeof(Enemy), typeof(SpriteRenderer))]
    public class EnemyView : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int HealthId = Shader.PropertyToID("_HealthFrac");
        private static readonly int SpikesId = Shader.PropertyToID("_Spikes");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        private Enemy enemy;
        private SpriteRenderer sr;
        private MaterialPropertyBlock block;
        private float flash;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            sr = GetComponent<SpriteRenderer>();
            block = new MaterialPropertyBlock();
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
            Apply();
        }

        private void HandleDamaged(Enemy e) => flash = 1f;

        private void Update()
        {
            if (flash <= 0f) return;
            flash = Mathf.Max(0f, flash - Time.deltaTime * 10f);
            Apply();
        }

        private void Apply()
        {
            block.SetFloat(FlashId, flash);
            block.SetFloat(HealthId, (float)enemy.Health / enemy.MaxHealth);
            sr.SetPropertyBlock(block);
        }
    }
}
