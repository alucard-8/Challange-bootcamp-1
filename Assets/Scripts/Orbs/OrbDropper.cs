using Soulbreak.Enemies;
using UnityEngine;

namespace Soulbreak.Orbs
{
    // Observer: listens for enemy deaths and rolls for orb drops.
    public class OrbDropper : MonoBehaviour
    {
        [SerializeField] private Sprite orbSprite;
        [SerializeField] private Color soulColor = new Color(0.7f, 0.4f, 1f);
        [SerializeField] private Color healColor = new Color(0.3f, 1f, 0.5f);
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float size = 0.6f;

        private void OnEnable() => Enemy.OnAnyKilled += HandleEnemyKilled;
        private void OnDisable() => Enemy.OnAnyKilled -= HandleEnemyKilled;

        private void HandleEnemyKilled(Enemy enemy)
        {
            var cfg = enemy.Config;
            Vector2 pos = enemy.transform.position;

            if (Random.value < cfg.healOrbChance)
                Spawn(OrbType.Heal, 1, pos, healColor);
            else if (Random.value < cfg.soulOrbChance)
                Spawn(OrbType.Soul, cfg.soulOrbValue, pos, soulColor);
        }

        private void Spawn(OrbType type, int value, Vector2 pos, Color color)
        {
            var go = new GameObject($"{type}Orb");
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = orbSprite;
            sr.sortingOrder = 2;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            go.AddComponent<Orb>().Init(type, value, lifetime, size, color);
        }
    }
}
