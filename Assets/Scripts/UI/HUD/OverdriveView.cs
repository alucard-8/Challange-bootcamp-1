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

        [SerializeField] private Text label;
        [SerializeField] private RectTransform fill;

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
