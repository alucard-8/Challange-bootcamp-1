using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Bottom-right: Nova ability name + cooldown bar.
    public class NovaView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private NovaAbility nova;

        private Text label;
        private RectTransform fill;

        private void Awake() => Build();

        private void Update()
        {
            float ready = nova.Cooldown > 0f ? 1f - nova.CooldownRemaining / nova.Cooldown : 1f;
            UIFactory.SetBar(fill, nova.IsReady ? 1f : ready);
            label.text = nova.IsReady ? "NOVA  <b>[SPACE]</b>" : $"NOVA  {nova.CooldownRemaining:0.0}s";
            label.color = nova.IsReady ? HudColors.Cyan : new Color(1f, 1f, 1f, 0.5f);
        }

        private void Build()
        {
            var br = UIFactory.StyledPanel(hud.Root, HudColors.Panel, HudColors.Cyan, "NovaPanel").rectTransform
                .Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 24), new Vector2(300, 80));
            label = UIFactory.Label(br, "NOVA", 26, TextAnchor.MiddleLeft, HudColors.Cyan, FontStyle.Bold);
            label.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -8), new Vector2(270, 34));
            fill = UIFactory.Bar(br, new Color(0, 0, 0, 0.6f), HudColors.Cyan, "NovaBar");
            fill.parent.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -50), new Vector2(264, 14));
        }
    }
}
