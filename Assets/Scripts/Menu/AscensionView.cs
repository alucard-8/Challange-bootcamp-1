using System.Collections.Generic;
using Soulbreak.Meta;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    // Main-menu "Ascension" screen: spend Soul Essence on starting techniques and reactor skins.
    // Rules live in MetaProgress; this view only displays and forwards clicks. Previews skins on the menu reactor.
    public class AscensionView : MonoBehaviour
    {
        private static readonly int CoreColorId = Shader.PropertyToID("_Color");
        private static readonly int ShieldColorId = Shader.PropertyToID("_ShieldColor");

        [SerializeField] private MetaCatalog catalog;
        [SerializeField] private SpriteRenderer menuReactor;

        public bool IsOpen { get; private set; }

        private class Entry { public MetaItem item; public Text title, body; public Button button; public Text buttonText; public Image card; }

        private readonly List<Entry> entries = new List<Entry>();
        private CanvasGroup group;
        private Text essenceText;
        private MaterialPropertyBlock block;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Build();
        }

        private void OnEnable() => MetaProgress.OnChanged += Refresh;
        private void OnDisable() => MetaProgress.OnChanged -= Refresh;

        private void Start()
        {
            Refresh();
            PreviewSkin();
        }

        public void Open() { IsOpen = true; Refresh(); }
        public void Close() => IsOpen = false;

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            group.blocksRaycasts = group.interactable = IsOpen;
        }

        private void Click(Entry e)
        {
            if (!MetaProgress.IsUnlocked(e.item)) MetaProgress.TryUnlock(e.item);
            else MetaProgress.ToggleEquip(e.item);
            PreviewSkin();
        }

        private void PreviewSkin()
        {
            if (menuReactor == null) return;
            var skin = catalog.Find(MetaProgress.EquippedSkin);
            menuReactor.GetPropertyBlock(block);
            if (skin != null)
            {
                block.SetColor(CoreColorId, skin.coreColor);
                block.SetColor(ShieldColorId, skin.shieldColor);
            }
            menuReactor.SetPropertyBlock(block);
        }

        private void Refresh()
        {
            essenceText.text = $"{MetaProgress.Essence} SOUL ESSENCE      BEST WAVE {MetaProgress.BestWave}";
            foreach (var e in entries)
            {
                bool unlocked = MetaProgress.IsUnlocked(e.item);
                bool equipped = MetaProgress.IsEquipped(e.item);
                e.buttonText.text = !unlocked ? $"UNLOCK  {e.item.cost}" : equipped ? (e.item.type == MetaItemType.ReactorSkin ? "EQUIPPED" : "EQUIPPED  (click to remove)") : "EQUIP";
                e.button.interactable = unlocked || MetaProgress.Essence >= e.item.cost;
                e.card.color = equipped ? new Color(0.12f, 0.08f, 0.22f, 0.97f) : new Color(0.06f, 0.04f, 0.11f, 0.95f);
            }
        }

        // ---------- Layout ----------
        private void Build()
        {
            var theme = UIFactory.Theme;
            var root = (RectTransform)UIFactory.Canvas("Ascension Canvas", transform, 15).transform;
            var bg = UIFactory.Panel(root, new Color(0.02f, 0.01f, 0.05f, 0.92f), "AscensionBg");
            bg.rectTransform.Fill();
            bg.raycastTarget = true;
            group = bg.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            var title = UIFactory.Label(bg.transform, "ASCENSION", 90, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1200, 110));
            title.gameObject.AddComponent<GlitchText>();
            essenceText = UIFactory.Label(bg.transform, "", 32, TextAnchor.MiddleCenter, theme.soul, FontStyle.Bold);
            essenceText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(1200, 44));

            var techs = new List<MetaItem>();
            var skins = new List<MetaItem>();
            foreach (var i in catalog.items) (i.type == MetaItemType.StartingTechnique ? techs : skins).Add(i);

            Section(bg.transform, "STARTING TECHNIQUE", techs, 230f);
            Section(bg.transform, "REACTOR SKIN", skins, 560f);

            var back = UIFactory.Button(bg.transform, "BACK", theme.buttonColor, Close, out _, 30);
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(300, 72));
        }

        private void Section(Transform parent, string header, List<MetaItem> items, float top)
        {
            var h = UIFactory.Label(parent, header, 28, TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.6f), FontStyle.Bold);
            h.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -top), new Vector2(1200, 40));

            const float w = 250f, gap = 18f;
            float total = items.Count * w + (items.Count - 1) * gap;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var e = new Entry { item = item };
                Color accent = item.type == MetaItemType.ReactorSkin ? Normalize(item.coreColor) : (item.technique != null ? Normalize(item.technique.color) : Color.white);

                e.card = UIFactory.StyledPanel(parent, new Color(0.06f, 0.04f, 0.11f, 0.95f), accent, item.id, 16f, true);
                var rt = e.card.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                    new Vector2(-total / 2f + w / 2f + i * (w + gap), -top - 50f), new Vector2(w, 250f));
                e.card.gameObject.AddComponent<UIButtonFx>().hoverScale = 1.04f;
                e.card.raycastTarget = true;

                e.title = UIFactory.Label(rt, item.displayName, 26, TextAnchor.UpperCenter, accent, FontStyle.Bold);
                e.title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(w - 24, 64));
                e.body = UIFactory.Label(rt, item.description, 19, TextAnchor.UpperCenter, new Color(1, 1, 1, 0.7f));
                e.body.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(w - 28, 100));

                var captured = e;
                e.button = UIFactory.Button(rt, "", new Color(0.3f, 0.18f, 0.5f, 0.95f), () => Click(captured), out e.buttonText, 20);
                ((RectTransform)e.button.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(w - 28, 52));
                entries.Add(e);
            }
        }

        private static Color Normalize(Color c)
        {
            float m = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            return new Color(c.r / m, c.g / m, c.b / m, 1f);
        }
    }
}
