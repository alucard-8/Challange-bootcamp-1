using Soulbreak.Enemies;
using Soulbreak.Orbs;
using Soulbreak.Player;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Fx
{
    // Pure observer: turns gameplay events into visuals (tracers, shockwaves, dissolving corpses).
    // Remove this object and the game still plays exactly the same — just without the juice.
    public class FxSystem : MonoBehaviour
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int SpikesId = Shader.PropertyToID("_Spikes");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private NovaAbility nova;
        [SerializeField] private Reactor.Reactor reactor;

        [Header("Event channels")]
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

        private void OnEnable()
        {
            shooter.OnShotFired += HandleShot;
            nova.OnUsed += HandleNova;
            reactor.OnHit += HandleReactorHit;
            enemyKilled.OnRaised += HandleEnemyKilled;
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
                // Projectile weapons: muzzle flash on the ring instead of a tracer.
                Ring(from + (to - from).normalized * 1.9f, c, 0.3f, 1.4f, 0.25f);
                return;
            }
            var go = new GameObject("Tracer", typeof(LineRenderer), typeof(TracerFx));
            go.GetComponent<TracerFx>().Init(from, to, tracerMaterial, c, crit ? 0.35f : 0.22f, 0.12f);
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
            var go = new GameObject("Lance", typeof(LineRenderer), typeof(TracerFx));
            go.GetComponent<TracerFx>().Init(beam.From, beam.To, tracerMaterial, beam.Color, beam.Width * 3.5f, 0.35f);
            Ring(beam.From, beam.Color, 0.5f, 2.2f, 0.3f);
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

        // A copy of the dead enemy that burns away with the shader's dissolve effect.
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
            var go = new GameObject("RingFx");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = quad;
            sr.sharedMaterial = ringMaterial;
            sr.sortingOrder = 7;

            var block = new MaterialPropertyBlock();
            block.SetColor(ColorId, color);
            block.SetFloat(Shader.PropertyToID("_Thickness"), thickness);
            go.AddComponent<FxAnimator>().Init(block, "_Progress", duration, Vector3.one * startSize, Vector3.one * endSize);
        }
    }
}
