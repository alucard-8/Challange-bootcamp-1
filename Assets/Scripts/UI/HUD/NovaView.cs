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

        [SerializeField] private Text label;
        [SerializeField] private RectTransform fill;


        private void Update()
        {
            float ready = nova.Cooldown > 0f ? 1f - nova.CooldownRemaining / nova.Cooldown : 1f;
            UIFactory.SetBar(fill, nova.IsReady ? 1f : ready);
            label.text = nova.IsReady ? $"{Lex.T("nova", "NOVA")}  <b>[SPACE]</b>" : $"{Lex.T("nova", "NOVA")}  {nova.CooldownRemaining:0.0}s";
            label.color = nova.IsReady ? HudColors.Cyan : new Color(1f, 1f, 1f, 0.5f);
        }
    }
}
