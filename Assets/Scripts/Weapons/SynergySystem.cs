using System.Collections.Generic;
using Soulbreak.Core.Events;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Weapons
{
    // Observer: watches the ring layout for newly completed synergies (announces them) and, whenever a
    // trigger module fires, fires the synergy's bonus pattern at the same target.
    public class SynergySystem : MonoBehaviour
    {
        [SerializeField] private WeaponRing ring;
        [SerializeField] private PlayerShooter shooter;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private SynergyDefinition[] synergies;
        [SerializeField] private AnnouncementEventChannel announcements;

        private readonly HashSet<SynergyDefinition> active = new HashSet<SynergyDefinition>();
        private bool firingBonus;

        private void OnEnable()
        {
            ring.OnLayoutChanged += HandleLayout;
            shooter.OnShotFired += HandleShot;
        }

        private void OnDisable()
        {
            ring.OnLayoutChanged -= HandleLayout;
            shooter.OnShotFired -= HandleShot;
        }

        private void HandleLayout()
        {
            foreach (var s in synergies)
            {
                if (s == null || active.Contains(s) || !s.IsActive(ring)) continue;
                active.Add(s);
                announcements.Raise(new Announcement($"SYNERGY: {s.displayName.ToUpper()}", s.description, new Color(0.5f, 1f, 0.7f)));
            }
        }

        private void HandleShot(Vector2 from, Vector2 to, bool crit, ShotPattern pattern)
        {
            if (firingBonus) return;   // bonus shots never chain into more bonus shots
            foreach (var s in active)
            {
                if (s.trigger != pattern || s.bonus == null) continue;
                firingBonus = true;
                Vector2 dir = (to - from).normalized;
                s.bonus.Fire(new ShotContext
                {
                    Origin = from,
                    Target = to,
                    Direction = dir,
                    Damage = Mathf.Max(1, Mathf.RoundToInt(stats.Get(StatType.Damage) * s.bonusDamageMultiplier)),
                    Crit = crit,
                    Stats = stats,
                });
                firingBonus = false;
            }
        }

        // Shop hint: "Synergy with X: Name" lines for a module.
        public string HintFor(ShotPattern module)
        {
            if (module == null || synergies == null) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var s in synergies)
                if (s != null && s.Involves(module))
                    sb.Append($"\n<color=#7CFFB0>+ {s.Partner(module).displayName}: {s.displayName}</color>");
            return sb.ToString();
        }
    }
}
