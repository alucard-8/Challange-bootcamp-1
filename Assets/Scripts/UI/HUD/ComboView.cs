using Soulbreak.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Top-center: combo counter + multiplier + time-left bar.
    public class ComboView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private ComboSystem combo;

        [SerializeField] private Text label;
        [SerializeField] private RectTransform fill;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private void OnEnable() => combo.OnComboChanged += HandleCombo;
        private void OnDisable() => combo.OnComboChanged -= HandleCombo;

        private void HandleCombo(int count, float mult) =>
            label.text = count >= 2 ? string.Format(S.hudCombo, count, UIFactory.Hex(HudColors.Gold), mult) : "";

        private void Update() => UIFactory.SetBar(fill, combo.Count >= 2 ? combo.TimeLeft01 : 0f);
    }
}
