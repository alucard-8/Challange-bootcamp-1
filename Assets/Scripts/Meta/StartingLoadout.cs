using Soulbreak.Reactor;
using Soulbreak.Weapons;
using UnityEngine;

namespace Soulbreak.Meta
{
    public class StartingLoadout : MonoBehaviour
    {
        [SerializeField] private MetaCatalog catalog;
        [SerializeField] private WeaponRing ring;
        [SerializeField] private ReactorView reactorView;

        private void Start()
        {
            var tech = catalog.Find(MetaProgress.EquippedTechnique);
            if (tech != null && tech.technique != null) ring.AddModule(tech.technique);

            var skin = catalog.Find(MetaProgress.EquippedSkin);
            if (skin != null) reactorView.SetColors(skin.coreColor, skin.shieldColor);
        }
    }
}
