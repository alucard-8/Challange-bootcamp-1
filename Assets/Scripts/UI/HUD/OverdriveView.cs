using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Bottom-right (above Nova): the Overdrive soul meter. Pulses when ready.
    public class OverdriveView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private OverdriveSystem overdrive;

        [SerializeField] private Text label;
        [SerializeField] private RectTransform fill;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private void Update()
        {
            UIFactory.SetBar(fill, overdrive.Meter01);
            if (overdrive.IsActive)
            {
                label.text = string.Format(S.hudOverdriveTimer, overdrive.Remaining);
                label.color = Color.white;
            }
            else if (overdrive.IsReady)
            {
                label.text = S.hudOverdriveReady;
                float pulse = 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 8f);
                label.color = Color.Lerp(T.purple, Color.white, pulse);
            }
            else
            {
                label.text = string.Format(S.hudOverdrivePercent, overdrive.Meter01 * 100f);
                label.color = T.dimText;
            }
        }
    }
}
