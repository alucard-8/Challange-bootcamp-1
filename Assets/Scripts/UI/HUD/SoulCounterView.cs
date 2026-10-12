using Soulbreak.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Top-right rolling soul counter + "+N" popups where souls were earned.
    public class SoulCounterView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private SoulWallet wallet;

        private Text soulsText;
        private int displayed, target;

        private void Awake() => Build();

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

        private void HandleSouls(int souls) => target = souls;

        private void HandleSoulsGained(int amount, Vector3 worldPos)
        {
            var cam = Camera.main;
            if (cam == null) return;
            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 local);
            var t = UIFactory.Label(hud.Root, $"+{amount}", amount >= 30 ? 40 : 28, TextAnchor.MiddleCenter, HudColors.Soul, FontStyle.Bold, "SoulPopup");
            t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            t.rectTransform.sizeDelta = new Vector2(200f, 60f);
            t.rectTransform.anchoredPosition = local;
            t.gameObject.AddComponent<FloatingText>();
        }

        private void Update()
        {
            if (displayed == target) return;
            int step = Mathf.Max(1, Mathf.Abs(target - displayed) / 8);
            displayed += displayed < target ? Mathf.Min(step, target - displayed) : -Mathf.Min(step, displayed - target);
            soulsText.text = $"{displayed}";
        }

        private void Build()
        {
            var tr = UIFactory.StyledPanel(hud.Root, HudColors.Panel, HudColors.Soul, "SoulPanel").rectTransform
                .Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -24), new Vector2(300, 110));
            UIFactory.Label(tr, "SOULS", 22, TextAnchor.UpperRight, new Color(1, 1, 1, 0.6f))
                .rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -10), new Vector2(260, 28));
            soulsText = UIFactory.Label(tr, "0", 56, TextAnchor.UpperRight, HudColors.Soul, FontStyle.Bold);
            soulsText.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -36), new Vector2(260, 64));
        }
    }
}
