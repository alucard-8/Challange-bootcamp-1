using System.Collections.Generic;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Bottom-center: one pip per bullet + reload progress bar.
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private AmmoSystem ammo;

        private RectTransform row, reloadFill;
        private Text reloadText;
        private readonly List<Image> pips = new List<Image>();

        private void Awake() => Build();
        private void OnEnable() => ammo.OnAmmoChanged += HandleAmmo;
        private void OnDisable() => ammo.OnAmmoChanged -= HandleAmmo;

        private void HandleAmmo(int cur, int max)
        {
            UIFactory.Pips(row, pips, max, 18f, 6f, HudColors.Dim, 46f);
            for (int i = 0; i < pips.Count; i++) pips[i].color = i < cur ? HudColors.Gold : HudColors.Dim;
        }

        private void Update()
        {
            reloadText.text = ammo.IsReloading ? "RELOADING" : ammo.Current == 0 ? "R / RMB to reload" : "";
            reloadText.color = ammo.IsReloading ? HudColors.Gold : new Color(1f, 1f, 1f, 0.5f);
            UIFactory.SetBar(reloadFill, ammo.IsReloading ? ammo.ReloadProgress : 0f);
        }

        private void Build()
        {
            var bc = UIFactory.Rect("AmmoPanel", hud.Root).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(600, 110));
            row = UIFactory.Rect("AmmoPips", bc).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(600, 50));
            reloadFill = UIFactory.Bar(bc, new Color(0, 0, 0, 0.5f), HudColors.Gold, "ReloadBar");
            reloadFill.parent.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(240, 8));
            reloadText = UIFactory.Label(bc, "", 20, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            reloadText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -76), new Vector2(400, 28));
        }
    }
}
