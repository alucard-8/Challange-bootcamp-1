using Soulbreak.Audio;
using Soulbreak.Enemies;
using Soulbreak.Fx;
using Soulbreak.MapEvents;
using Soulbreak.Orbs;
using Soulbreak.Perks;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Mediator that configures the game scene for the selected mode before anything else runs.
    // Each system just receives its data through a small setter — none of them know modes exist.
    [DefaultExecutionOrder(-200)]
    public class ModeApplier : MonoBehaviour
    {
        [SerializeField] private WaveManager waves;
        [SerializeField] private EnemySplitter splitter;
        [SerializeField] private MapEventSystem mapEvents;
        [SerializeField] private PerkShop shop;
        [SerializeField] private OrbDropper dropper;
        [SerializeField] private FxSystem fx;
        [SerializeField] private MusicSynth music;
        [SerializeField] private SpriteRenderer core;
        [SerializeField] private SpriteRenderer background;

        private void Awake()
        {
            var mode = GameModeService.Current;
            if (mode == null) return;

            if (mode.roster != null && mode.roster.Length > 0) waves.SetRoster(mode.roster, mode.boss);
            if (mode.eliteSplitInto != null) splitter.SetEliteSplit(mode.eliteSplitInto);
            if (mode.rejuvenator != null) mapEvents.SetRejuvenator(mode.rejuvenator);
            if (mode.catalog != null && mode.catalog.Length > 0) shop.SetCatalog(mode.catalog);
            dropper.SetColors(mode.soulOrbColor, mode.healOrbColor);
            if (mode.enemyMaterial != null) fx.SetEnemyMaterial(mode.enemyMaterial);
            if (mode.coreMaterial != null) core.sharedMaterial = mode.coreMaterial;
            if (mode.backgroundMaterial != null) background.sharedMaterial = mode.backgroundMaterial;
            music.SetVariant(mode.musicVariant);
        }
    }
}
