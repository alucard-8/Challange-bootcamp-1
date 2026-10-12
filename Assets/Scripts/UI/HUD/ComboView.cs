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

        private Text label;
        private RectTransform fill;

        private void Awake() => Build();
        private void OnEnable() => combo.OnComboChanged += HandleCombo;
        private void OnDisable() => combo.OnComboChanged -= HandleCombo;

        private void HandleCombo(int count, float mult) =>
            label.text = count >= 2 ? $"COMBO {count}   <color=#{UIFactory.Hex(HudColors.Gold)}>x{mult:0.00}</color>" : "";

        private void Update() => UIFactory.SetBar(fill, combo.Count >= 2 ? combo.TimeLeft01 : 0f);

        private void Build()
        {
            label = UIFactory.Label(hud.Root, "", 34, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, "Combo");
            label.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(600, 44));
            fill = UIFactory.Bar(hud.Root, new Color(0, 0, 0, 0), HudColors.Gold, "ComboBar");
            fill.parent.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -78), new Vector2(260, 8));
        }
    }
}
