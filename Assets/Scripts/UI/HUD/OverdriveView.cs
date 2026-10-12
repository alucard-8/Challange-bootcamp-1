using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Bottom-right (above Nova): the Overdrive soul meter. Pulses when ready.
    public class OverdriveView : MonoBehaviour
    {
        private static readonly Color Purple = new Color(0.85f, 0.4f, 1f);

        [SerializeField] private HUDCanvas hud;
        [SerializeField] private OverdriveSystem overdrive;

        private Text label;
        private RectTransform fill;

        private void Awake()
        {
            var panel = UIFactory.StyledPanel(hud.Root, HudColors.Panel, Purple, "OverdrivePanel").rectTransform
                .Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-24, 116), new Vector2(300, 80));
            label = UIFactory.Label(panel, "OVERDRIVE", 26, TextAnchor.MiddleLeft, Purple, FontStyle.Bold);
            label.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -8), new Vector2(270, 34));
            fill = UIFactory.Bar(panel, new Color(0, 0, 0, 0.6f), Purple, "OverdriveBar");
            fill.parent.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -50), new Vector2(264, 14));
        }

        private void Update()
        {
            UIFactory.SetBar(fill, overdrive.Meter01);
            if (overdrive.IsActive)
            {
                label.text = $"OVERDRIVE  {overdrive.Remaining:0.0}s";
                label.color = Color.white;
            }
            else if (overdrive.IsReady)
            {
                label.text = "OVERDRIVE  <b>[Q]</b>";
                float pulse = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 8f);
                label.color = Color.Lerp(Purple, Color.white, pulse);
            }
            else
            {
                label.text = $"OVERDRIVE  {overdrive.Meter01 * 100f:0}%";
                label.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }
    }
}
