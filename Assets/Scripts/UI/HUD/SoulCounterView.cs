using Soulbreak.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Top-right soul counter. Earned souls fly to it as glowing motes; the number only climbs as they
    // land (with a bump), so you SEE souls arriving. Spending updates it immediately.
    public class SoulCounterView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private SoulWallet wallet;

        [SerializeField] private Text soulsText;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Text popupTemplate;
        [SerializeField] private FlyingMote moteTemplate;
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        private int displayed, target, actual;
        private bool initialized;
        private float bump;


        private void OnEnable()
        {
            wallet.OnSoulsChanged += HandleSouls;
            wallet.OnSoulsGained += HandleSoulsGained;
        }

        private void OnDisable()
        {
            wallet.OnSoulsChanged -= HandleSouls;
            wallet.OnSoulsGained -= HandleSoulsGained;
        }

        private void HandleSouls(int souls)
        {
            if (!initialized) { initialized = true; actual = target = displayed = souls; soulsText.text = $"{souls}"; return; }
            if (souls < actual) target -= actual - souls;   // spending shows instantly; gains arrive by mote
            actual = souls;
        }

        private void HandleSoulsGained(int amount, Vector3 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) { target += amount; return; }

            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 from);

            // "+N" popup where it was earned
            var t = UIFactory.Spawn(popupTemplate, hud.Root);
            t.text = string.Format(S.hudCollect, amount);
            t.fontSize = amount >= 30 ? 40 : 28;
            t.rectTransform.anchoredPosition = from;

            // Motes flying to the counter, each carrying a share of the souls
            Vector2 to = hud.Root.InverseTransformPoint(soulsText.rectTransform.position);
            int motes = Mathf.Clamp(amount / 6, 1, 6);
            int share = amount / motes, rest = amount - share * motes;
            for (int i = 0; i < motes; i++)
            {
                int carried = share + (i == 0 ? rest : 0);
                UIFactory.Spawn(moteTemplate, hud.Root).Launch(from, to, i * 0.05f, () => Arrive(carried));
            }
        }

        private void Arrive(int amount)
        {
            target += amount;
            bump = 1f;
        }

        private void Update()
        {
            if (displayed != target)
            {
                int step = Mathf.Max(1, Mathf.Abs(target - displayed) / 6);
                displayed += displayed < target ? Mathf.Min(step, target - displayed) : -Mathf.Min(step, displayed - target);
                soulsText.text = $"{displayed}";
            }

            bump = Mathf.Max(0f, bump - Time.unscaledDeltaTime * 6f);
            panel.localScale = Vector3.one * (1f + 0.08f * bump);
            soulsText.color = Color.Lerp(HudColors.Soul, Color.white, bump * 0.7f);
        }
    }
}
