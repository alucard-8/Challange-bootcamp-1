using Soulbreak.Core.Events;
using Soulbreak.Enemies;
using Soulbreak.Perks;
using UnityEngine;

namespace Soulbreak.Meta
{
    public class CodexRecorder : MonoBehaviour
    {
        [SerializeField] private EnemyEventChannel enemySpawned;
        [SerializeField] private PerkShop shop;
        [SerializeField] private AnnouncementEventChannel announcements;

        private void OnEnable()
        {
            enemySpawned.OnRaised += HandleSpawn;
            shop.OnOffersChanged += HandleOffers;
        }

        private void OnDisable()
        {
            enemySpawned.OnRaised -= HandleSpawn;
            shop.OnOffersChanged -= HandleOffers;
        }

        private void HandleSpawn(Enemy e)
        {
            if (e.Config == null || !MetaProgress.Discover(true, e.Config.name)) return;
            if (announcements != null)
                announcements.Raise(new Announcement("NEW ENEMY", CodexCatalog.Name(e.Config) + "  -  added to the Index", new Color(1f, 0.79f, 0.3f)));
        }

        private void HandleOffers()
        {
            foreach (var offer in shop.Offers)
                if (offer.Perk != null) MetaProgress.Discover(false, offer.Perk.name);
        }
    }
}
