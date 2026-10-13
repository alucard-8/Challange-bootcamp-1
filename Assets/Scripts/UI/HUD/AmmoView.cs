using System.Collections.Generic;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Bottom-center: one pip per bullet + a bar tracking the next shot's recharge. Pips blink red when low,
    // and the row pops gold when an ammo charge lands.
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private AmmoSystem ammo;

        [SerializeField] private RectTransform row, reloadFill;
        [SerializeField] private Text reloadText;
        [SerializeField] private Image pipTemplate;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private readonly List<Image> pips = new List<Image>();
        private float pickupFlash;
        private int current, max;

        private void OnEnable()
        {
            ammo.OnAmmoChanged += HandleAmmo;
            ammo.OnAmmoPickup += HandlePickup;
        }

        private void OnDisable()
        {
            ammo.OnAmmoChanged -= HandleAmmo;
            ammo.OnAmmoPickup -= HandlePickup;
        }

        private void HandlePickup() => pickupFlash = 1f;

        private void HandleAmmo(int cur, int max)
        {
            current = cur; this.max = max;
            UIFactory.Pips(pipTemplate, pips, max, 6f);
        }

        private void Update()
        {
            pickupFlash = Mathf.MoveTowards(pickupFlash, 0f, Time.unscaledDeltaTime * 1.6f);
            bool low = current > 0 && current <= Mathf.Max(1, max / 4);
            float blink = low ? 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 14f) : 0f;
            var lowColor = Color.Lerp(HudColors.Gold, T.lowAmmo, blink);
            for (int i = 0; i < pips.Count; i++)
                pips[i].color = i < current ? Color.Lerp(low ? lowColor : HudColors.Gold, Color.white, pickupFlash) : HudColors.Dim;
            row.localScale = Vector3.one * (1f + 0.25f * pickupFlash * pickupFlash);

            reloadText.text = pickupFlash > 0f ? S.hudAmmoCharge : current <= 0 ? S.hudReloading : low ? S.hudLowAmmo : "";
            reloadText.color = pickupFlash > 0f ? Color.Lerp(HudColors.Gold, Color.white, pickupFlash) : current <= 0 ? HudColors.Gold : low ? lowColor : T.dimText;
            UIFactory.SetBar(reloadFill, ammo.IsReloading ? ammo.ReloadProgress : 0f);
        }
    }
}
