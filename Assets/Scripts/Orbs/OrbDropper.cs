using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Orbs
{
    public class OrbDropper : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private OrbEventChannel orbCollected;
        [SerializeField] private Sprite orbSprite;
        [SerializeField] private Material orbMaterial;
        [SerializeField, ColorUsage(false, true)] private Color soulColor = new Color(1.6f, 0.7f, 3f);
        [SerializeField, ColorUsage(false, true)] private Color healColor = new Color(0.6f, 3f, 1.2f);
        [SerializeField] private float size = 0.7f;

        [Header("Ammo drops")]
        [SerializeField] private AmmoSystem ammo;
        [SerializeField] private Transform magnetTarget;
        [SerializeField, ColorUsage(false, true)] private Color ammoColor = new Color(3f, 1.9f, 0.4f);
        [SerializeField, Range(0, 1)] private float ammoChance = 0.1f;
        [SerializeField, Range(0, 1)] private float lowAmmoChance = 0.35f;

        public void SetColors(Color soul, Color heal) { soulColor = soul; healColor = heal; }

        private void OnEnable() => enemyKilled.OnRaised += HandleEnemyKilled;
        private void OnDisable() => enemyKilled.OnRaised -= HandleEnemyKilled;

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
                else if (Random.value < cfg.soulOrbChance + stats.Get(StatType.OrbDropChance) + (enemy.IsElite ? 0.6f : 0f))
                    Spawn(OrbType.Soul, Mathf.RoundToInt(cfg.soulOrbValue * stats.Get(StatType.OrbValue)), p, soulColor);
            }
            RollAmmo(pos, cfg.isBoss || enemy.IsElite);
        }

        public Orb SpawnFor(OrbType type, Vector2 pos)
        {
            switch (type)
            {
                case OrbType.Ammo:
                    var orb = Spawn(OrbType.Ammo, 1, pos, ammoColor);
                    orb.transform.localScale = Vector3.one * size * 0.8f;
                    if (magnetTarget != null) orb.Magnetize(magnetTarget, Random.insideUnitCircle.normalized * 3f);
                    return orb;
                case OrbType.Heal: return Spawn(OrbType.Heal, 1, pos, healColor);
                default: return Spawn(OrbType.Soul, 25, pos, soulColor);
            }
        }

        private void RollAmmo(Vector2 pos, bool guaranteed)
        {
            if (ammo == null || magnetTarget == null) return;
            bool low = ammo.Current <= Mathf.Max(1, ammo.Max / 3);
            if (!guaranteed && Random.value >= (low ? lowAmmoChance : ammoChance)) return;
            var orb = Spawn(OrbType.Ammo, 1, pos, ammoColor);
            orb.transform.localScale = Vector3.one * size * 0.8f;
            orb.Magnetize(magnetTarget, Random.insideUnitCircle.normalized * 5f);
        }

        private Orb Spawn(OrbType type, int value, Vector2 pos, Color color)
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

            var orb = go.AddComponent<Orb>();
            orb.Init(type, value, stats.Get(StatType.OrbLifetime), color, orbCollected);
            return orb;
        }
    }
}
