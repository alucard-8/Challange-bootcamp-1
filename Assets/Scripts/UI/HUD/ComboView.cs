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
        private void OnEnable() => combo.OnComboChanged += HandleCombo;
        private void OnDisable() => combo.OnComboChanged -= HandleCombo;

        private void HandleCombo(int count, float mult) =>
            label.text = count >= 2 ? $"COMBO {count}   <color=#{UIFactory.Hex(HudColors.Gold)}>x{mult:0.00}</color>" : "";

        private void Update() => UIFactory.SetBar(fill, combo.Count >= 2 ? combo.TimeLeft01 : 0f);
    }
}
