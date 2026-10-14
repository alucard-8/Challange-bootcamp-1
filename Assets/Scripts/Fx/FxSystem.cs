using Soulbreak.Enemies;
using Soulbreak.Orbs;
using Soulbreak.Player;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Fx
{
    public class FxSystem : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int SpikesId = Shader.PropertyToID("_Spikes");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int ThicknessId = Shader.PropertyToID("_Thickness");

        private readonly System.Collections.Generic.Stack<GameObject> ringPool = new System.Collections.Generic.Stack<GameObject>();
        private readonly System.Collections.Generic.Stack<GameObject> tracerPool = new System.Collections.Generic.Stack<GameObject>();

        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private Perks.PerkShop shop;

        [Header("Event channels")]
        [SerializeField] private EnemyEventChannel bossEnraged;
        [SerializeField] private EnemyDamageEventChannel enemyDamaged;
        [SerializeField] private Material sparkMaterial;
        [SerializeField] private EnemyEventChannel enemyKilled;
        [SerializeField] private OrbEventChannel orbCollected;
        [SerializeField] private EnemyProjectileEventChannel projectileShotDown;
        [SerializeField] private ExplosionEventChannel explosions;
        [SerializeField] private ChainEventChannel chains;
        [SerializeField] private BeamEventChannel beams;

        [Header("Assets")]
        [SerializeField] private Sprite quad;
        [SerializeField] private Material ringMaterial;
        [SerializeField] private Material tracerMaterial;
        [SerializeField] private Material enemyMaterial;

        [Header("Colors (HDR)")]
        [SerializeField, ColorUsage(true, true)] private Color shotColor = new Color(1.2f, 2.2f, 4f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color critColor = new Color(4f, 2.4f, 0.6f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color novaColor = new Color(1.5f, 2.5f, 4f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color shieldHitColor = new Color(0.6f, 2.5f, 3f, 1f);
        [SerializeField, ColorUsage(true, true)] private Color coreHitColor = new Color(4f, 0.5f, 0.5f, 1f);

        public void SetEnemyMaterial(Material material) => enemyMaterial = material;

        private void OnEnable()
        {
            shooter.OnShotFired += HandleShot;
            nova.OnUsed += HandleNova;
            reactor.OnHit += HandleReactorHit;
            enemyKilled.OnRaised += HandleEnemyKilled;
            bossEnraged.OnRaised += HandleBossEnraged;
            enemyDamaged.OnRaised += HandleEnemyDamaged;
            shop.OnPerkBought += HandlePerkBought;
            orbCollected.OnRaised += HandleOrbCollected;
            projectileShotDown.OnRaised += HandleProjectileShotDown;
            explosions.OnRaised += HandleExplosion;
            chains.OnRaised += HandleChain;
            beams.OnRaised += HandleBeam;
        }

        private void OnDisable()
        {
            shooter.OnShotFired -= HandleShot;
            nova.OnUsed -= HandleNova;
            reactor.OnHit -= HandleReactorHit;
            enemyKilled.OnRaised -= HandleEnemyKilled;
            bossEnraged.OnRaised -= HandleBossEnraged;
            enemyDamaged.OnRaised -= HandleEnemyDamaged;
            shop.OnPerkBought -= HandlePerkBought;
            orbCollected.OnRaised -= HandleOrbCollected;
            projectileShotDown.OnRaised -= HandleProjectileShotDown;
            explosions.OnRaised -= HandleExplosion;
            chains.OnRaised -= HandleChain;
            beams.OnRaised -= HandleBeam;
        }

        private void HandleShot(Vector2 from, Vector2 to, bool crit, ShotPattern pattern)
        {
            var c = crit ? critColor : (pattern.drawsTracer ? shotColor : pattern.color);
            if (!pattern.drawsTracer)
            {
                Ring(from + (to - from).normalized * 1.9f, c, 0.3f, 1.4f, 0.25f);
                return;
            }
            var go = GetTracer();
            go.GetComponent<TracerFx>().Init(from, to, tracerMaterial, c, crit ? 0.35f : 0.22f, 0.12f, () => ReturnTracer(go));
            Ring(to, c, 0.2f, crit ? 1.6f : 1f, 0.25f);
        }

        private void HandleExplosion(ExplosionInfo e) => Ring(e.Position, e.Color, 0.4f, e.Radius * 2.2f, 0.4f, 0.12f);

        private void HandleChain(ChainInfo chain)
        {
            var go = new GameObject("Lightning", typeof(LineRenderer), typeof(LightningFx));
            go.GetComponent<LightningFx>().Init(chain.Points, tracerMaterial, chain.Color, 0.28f, 0.3f);
            for (int i = 1; i < chain.Points.Count; i++) Ring(chain.Points[i], chain.Color, 0.2f, 1.4f, 0.3f);
        }

        private void HandleBeam(BeamInfo beam)
        {
            var go = GetTracer();
            go.GetComponent<TracerFx>().Init(beam.From, beam.To, tracerMaterial, beam.Color, beam.Width * 3.5f, 0.35f, () => ReturnTracer(go));
            Ring(beam.From, beam.Color, 0.5f, 2.2f, 0.3f);
        }

        private void HandleEnemyDamaged(EnemyDamage hit)
        {
            if (hit.Blocked)
            {
                Ring(hit.Position, new Color(0.6f, 2.4f, 3.6f), 0.4f, 1.8f, 0.25f, 0.2f);
                SparkFx.Burst(hit.Position, new Color(0.6f, 0.9f, 1f), 5, 3f, quad, sparkMaterial, 0.14f);
                return;
            }
            var c = hit.Enemy != null ? hit.Enemy.Config.color : Color.white;
            float m = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            SparkFx.Burst(hit.Position, new Color(c.r / m, c.g / m, c.b / m), hit.Killed ? 10 : 6, hit.Killed ? 6f : 4f, quad, sparkMaterial);
        }

        private void HandlePerkBought(Perks.PerkDefinition perk) =>
            Ring(reactor.transform.position, new Color(1.6f, 0.7f, 3f), 1f, 7f, 0.6f, 0.1f);

        private void HandleBossEnraged(Enemy boss)
        {
            Ring(boss.transform.position, new Color(4f, 0.4f, 0.6f), 1f, 12f, 0.8f, 0.08f);
            Ring(boss.transform.position, new Color(4f, 1.5f, 0.4f), 0.5f, 7f, 0.6f, 0.15f);
        }

        private void HandleNova(Vector2 center, float radius) => Ring(center, novaColor, 0.5f, radius * 2.2f, 0.6f, 0.08f);

        private void HandleReactorHit(bool shielded) =>
            Ring(reactor.transform.position, shielded ? shieldHitColor : coreHitColor, 1f, 4f, 0.45f);

        private void HandleEnemyKilled(Enemy enemy)
        {
            var cfg = enemy.Config;
            Ring(enemy.transform.position, cfg.color, cfg.size * 0.5f, cfg.size * (cfg.isBoss ? 5f : 2.5f), 0.35f);
            DissolveGhost(enemy);
        }

        private void HandleOrbCollected(Orb orb) => Ring(orb.transform.position, orb.Color, 0.3f, 1.8f, 0.35f);

        private void HandleProjectileShotDown(EnemyProjectile p) => Ring(p.transform.position, critColor, 0.2f, 1f, 0.25f);

        private void DissolveGhost(Enemy enemy)
        {
            var go = new GameObject("DissolveFx");
            go.transform.position = enemy.transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = enemyMaterial;
            sr.sortingOrder = 2;

            var block = new MaterialPropertyBlock();
            block.SetColor(ColorId, enemy.Config.color);
            block.SetFloat(SpikesId, enemy.Config.spikes);
            block.SetFloat(SeedId, Random.value * 10f);

            Vector3 s = enemy.transform.localScale;
            go.AddComponent<FxAnimator>().Init(block, "_Dissolve", 0.45f, s, s * 1.15f);
        }

        private void Ring(Vector2 pos, Color color, float startSize, float endSize, float duration, float thickness = 0.15f)
        {
            var go = GetRing();
            go.transform.position = pos;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = ringMaterial;
            sr.sortingOrder = 7;

            var block = new MaterialPropertyBlock();
            block.SetColor(ColorId, color);
            block.SetFloat(ThicknessId, thickness);
            go.GetComponent<FxAnimator>().Init(block, "_Progress", duration, Vector3.one * startSize, Vector3.one * endSize, () => ReturnRing(go));
        }

        private GameObject GetRing()
        {
            var go = ringPool.Count > 0 ? ringPool.Pop() : new GameObject("RingFx", typeof(SpriteRenderer), typeof(FxAnimator));
            go.SetActive(true);
            return go;
        }

        private void ReturnRing(GameObject go)
        {
            go.SetActive(false);
            ringPool.Push(go);
        }

        private GameObject GetTracer()
        {
            var go = tracerPool.Count > 0 ? tracerPool.Pop() : new GameObject("Tracer", typeof(LineRenderer), typeof(TracerFx));
            go.SetActive(true);
            return go;
        }

        private void ReturnTracer(GameObject go)
        {
            go.SetActive(false);
            tracerPool.Push(go);
        }
    }
}
