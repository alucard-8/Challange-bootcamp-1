using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class NovaView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private NovaAbility nova;

        [SerializeField] private Text label;
        [SerializeField] private RectTransform fill;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private void Update()
        {
            float ready = nova.Cooldown > 0f ? 1f - nova.CooldownRemaining / nova.Cooldown : 1f;
            UIFactory.SetBar(fill, nova.IsReady ? 1f : ready);
            string name = Lex.T("nova", "NOVA");
            label.text = nova.IsReady ? string.Format(S.hudNovaReadyLabel, name) : string.Format(S.hudNovaCooldown, name, nova.CooldownRemaining);
            label.color = nova.IsReady ? HudColors.Cyan : T.dimText;
        }
    }
}
