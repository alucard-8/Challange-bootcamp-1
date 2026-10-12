using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Orbs
{
    // Observer: listens for enemy deaths and rolls for orb drops (chances boosted by perks).
    public class OrbDropper : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Sprite orbSprite;
        [SerializeField] private Material orbMaterial;
        [SerializeField, ColorUsage(false, true)] private Color soulColor = new Color(1.6f, 0.7f, 3f);
        [SerializeField, ColorUsage(false, true)] private Color healColor = new Color(0.6f, 3f, 1.2f);
        [SerializeField] private float size = 0.7f;

        private void OnEnable() => Enemy.OnAnyKilled += HandleEnemyKilled;
        private void OnDisable() => Enemy.OnAnyKilled -= HandleEnemyKilled;

        private void HandleEnemyKilled(Enemy enemy)
        {
            var cfg = enemy.Config;
            Vector2 pos = enemy.transform.position;
            int drops = cfg.isBoss ? 5 : 1;

            for (int i = 0; i < drops; i++)
            {
                Vector2 p = pos + (drops > 1 ? Random.insideUnitCircle * 1.2f : Vector2.zero);
                if (Random.value < cfg.healOrbChance + stats.Get(StatType.HealOrbChance))
                    Spawn(OrbType.Heal, 1, p, healColor);
                else if (Random.value < cfg.soulOrbChance + stats.Get(StatType.OrbDropChance))
                    Spawn(OrbType.Soul, Mathf.RoundToInt(cfg.soulOrbValue * stats.Get(StatType.OrbValue)), p, soulColor);
            }
        }

        private void Spawn(OrbType type, int value, Vector2 pos, Color color)
        {
            var go = new GameObject($"{type}Orb");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = orbSprite;
            if (orbMaterial != null) sr.sharedMaterial = orbMaterial;
            sr.sortingOrder = 5;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<Orb>().Init(type, value, stats.Get(StatType.OrbLifetime), color);
        }
    }
}
