using System.Collections.Generic;
using Soulbreak.Core.Events;
using Soulbreak.Meta;
using Soulbreak.Perks;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Reactor
{
    // Makes the core look more "equipped" as the run goes on (both modes):
    //   offense perks  -> spikes / solar prominences       defense perks -> armor plates / asteroid belt
    //   economy perks  -> golden halo / bigger corona        techniques    -> satellites / moons orbiting the core
    //   every 4 perks  -> a new core tier (brighter, pulse, announcement)
    // Pure listener on the shop and the weapon ring; it forwards shader values to ReactorView.
    public class CoreUpgradeView : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;
        [SerializeField] private WeaponRing ring;
        [SerializeField] private ReactorView reactorView;
        [SerializeField] private AnnouncementEventChannel announcements;
        [SerializeField] private Sprite quad;
        [SerializeField] private Material satelliteMaterial;
        [SerializeField] private float satelliteRadius = 2.45f;

        private float offense, defense, economy, pulse;
        private int bought, tier;
        private readonly List<(Transform t, float speed, float phase, float radius)> satellites = new List<(Transform, float, float, float)>();

        private void OnEnable()
        {
            shop.OnPerkBought += HandleBought;
            ring.OnLayoutChanged += RebuildSatellites;
        }

        private void OnDisable()
        {
            shop.OnPerkBought -= HandleBought;
            ring.OnLayoutChanged -= RebuildSatellites;
        }

        private void Start() => Push();

        private void HandleBought(PerkDefinition perk)
        {
            switch (perk.category)
            {
                case PerkCategory.Offense: offense += 1f; break;
                case PerkCategory.Defense: defense += 1f; break;
                case PerkCategory.Economy: economy += 0.25f; break;
                case PerkCategory.Ability: offense += 0.5f; economy += 0.1f; break;
            }

            bought++;
            int newTier = bought / 4;
            if (newTier > tier)
            {
                tier = newTier;
                pulse = 1f;
                string what = GameModeService.Text("core", "CORE");
                announcements.Raise(new Announcement($"{what} TIER {tier + 1}", "Your core grows stronger", new Color(0.6f, 0.9f, 1f)));
            }
            Push();
        }

        private void Update()
        {
            if (pulse > 0f)
            {
                pulse = Mathf.Max(0f, pulse - Time.unscaledDeltaTime * 0.9f);
                Push();
            }

            float time = Time.time;
            Vector3 c = reactorView.transform.position;
            foreach (var s in satellites)
            {
                if (s.t == null) continue;
                float a = s.phase + time * s.speed;
                s.t.position = c + new Vector3(Mathf.Cos(a), Mathf.Sin(a) * 0.92f) * s.radius;
            }
        }

        private void Push() => reactorView.SetUpgrades(offense, defense, economy, tier, pulse);

        // One small satellite (moon in Star mode) per owned technique, in that technique's color.
        private void RebuildSatellites()
        {
            foreach (var s in satellites) if (s.t != null) Destroy(s.t.gameObject);
            satellites.Clear();

            int i = 0;
            foreach (var module in ring.OwnedModules)
            {
                var go = new GameObject($"Satellite_{module.displayName}");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = quad;
                sr.sharedMaterial = satelliteMaterial;
                // Dimmed so a full ring of satellites doesn't bloom into a haze.
                var c = module.color;
                float m = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
                sr.color = new Color(c.r / m, c.g / m, c.b / m, 0.55f);
                sr.sortingOrder = 1;
                go.transform.localScale = Vector3.one * (GameModeService.IsStar ? 0.32f : 0.24f);
                float radius = satelliteRadius + (i % 2) * 0.25f;
                satellites.Add((go.transform, 0.9f - i * 0.07f, i * 2.4f, radius));
                i++;
            }
        }
    }
}
