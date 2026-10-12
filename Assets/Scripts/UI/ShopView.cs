using System.Collections.Generic;

using Soulbreak.Economy;
using Soulbreak.Perks;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Draws the PerkShop as a row of cards and forwards clicks / number keys to it.
    // All rules (prices, rolling, buying) live in PerkShop — this class only displays.
    public class ShopView : MonoBehaviour
    {
        [SerializeField] private PerkShop shop;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private Weapons.SynergySystem synergies;

        private static readonly Color[] RarityColors =
        {
            new Color(0.65f, 0.72f, 0.82f),   // Common
            new Color(0.3f, 0.65f, 1f),       // Rare
            new Color(0.75f, 0.4f, 1f),       // Epic
            new Color(1f, 0.75f, 0.2f),       // Legendary
        };

        private static readonly Color[] CategoryColors =
        {
            new Color(1f, 0.45f, 0.4f),       // Offense
            new Color(0.8f, 0.55f, 1f),       // Economy
            new Color(0.4f, 0.85f, 1f),       // Defense
            new Color(1f, 0.85f, 0.35f),      // Ability
            new Color(0.45f, 1f, 0.6f),       // Weapon
        };

        private class Card
        {
            public RectTransform Root;
            public Image Background, RarityBar;
            public Text Category, Name, Description, Owned, Rarity, Hotkey, ButtonText;
            public Button Buy;
            public CanvasGroup Group, Intro;
        }

        private readonly List<Vector2> cardBase = new List<Vector2>();
        private float showTime;

        private CanvasGroup group;
        private Text titleText, soulsText, rerollText;
        private Button rerollButton;
        private RectTransform cardRow;
        private readonly List<Card> cards = new List<Card>();

        private void Awake() => Build();

        private void OnEnable()
        {
            shop.OnOpened += Show;
            shop.OnClosed += Hide;
            shop.OnOffersChanged += Refresh;
            wallet.OnSoulsChanged += HandleSouls;
        }

        private void OnDisable()
        {
            shop.OnOpened -= Show;
            shop.OnClosed -= Hide;
            shop.OnOffersChanged -= Refresh;
            wallet.OnSoulsChanged -= HandleSouls;
        }

        private void Update()
        {
            if (!shop.IsOpen) return;

            // Open animation: overlay fades in, cards fly up one after another.
            showTime += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(showTime * 4f);
            for (int i = 0; i < cards.Count; i++)
            {
                float t = Mathf.Clamp01((showTime - 0.12f - i * 0.07f) / 0.45f);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                cards[i].Intro.alpha = ease;
                cards[i].Root.anchoredPosition = cardBase[i] + Vector2.down * 120f * (1f - ease);
            }
        }

        private void Show()
        {
            showTime = 0f;
            group.alpha = 0f;
            group.blocksRaycasts = group.interactable = true;
            Refresh();
        }

        private void Hide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = group.interactable = false;
        }

        private void HandleSouls(int souls)
        {
            if (shop.IsOpen) Refresh();
        }

        private void Refresh()
        {
            soulsText.text = $"{wallet.Souls} SOULS";
            rerollText.text = $"REROLL  ({shop.RerollCost})";
            rerollButton.interactable = wallet.Souls >= shop.RerollCost;

            var offers = shop.Offers;
            EnsureCards(offers.Count);

            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                bool has = i < offers.Count;
                card.Root.gameObject.SetActive(has);
                if (!has) continue;

                var offer = offers[i];
                var perk = offer.Perk;
                var rarity = RarityColors[Mathf.Clamp((int)perk.rarity, 0, RarityColors.Length - 1)];
                var cat = CategoryColors[Mathf.Clamp((int)perk.category, 0, CategoryColors.Length - 1)];
                int stacks = shop.GetStacks(perk);
                bool affordable = wallet.Souls >= offer.Cost;

                card.RarityBar.color = rarity;
                // Buff cards and technique cards get different animated faces (UICard shader).
                var technique = perk.TechniqueEffect;
                var mat = card.Background.material;
                if (mat != null && mat.HasProperty("_Style"))
                {
                    mat.SetColor("_Accent", technique != null && technique.module != null ? (Color)technique.module.color * 0.45f + rarity * 0.55f : rarity);
                    mat.SetFloat("_Style", technique != null ? 1f : 0f);
                    mat.SetFloat("_Icon", technique != null && technique.module != null ? technique.module.icon : 0f);
                    mat.SetFloat("_Dim", offer.Sold ? 1f : 0f);
                }
                card.Rarity.text = perk.rarity.ToString().ToUpper();
                card.Rarity.color = rarity;
                card.Category.text = perk.IsTechnique ? "TECHNIQUE" : $"BUFF  -  {perk.category.ToString().ToUpper()}";
                card.Category.color = cat;
                card.Name.text = perk.displayName;
                var tech = perk.TechniqueEffect;
                card.Description.text = perk.description + (synergies != null && tech != null ? synergies.HintFor(tech.module) : "");
                card.Owned.text = perk.maxStacks > 0 ? $"Owned {stacks}/{perk.maxStacks}" : (stacks > 0 ? $"Owned {stacks}" : "");
                card.Hotkey.text = $"[{i + 1}]";

                card.Background.color = Color.Lerp(new Color(0.07f, 0.05f, 0.13f, 0.97f), rarity, 0.08f);
                card.Buy.interactable = !offer.Sold && affordable;
                card.ButtonText.text = offer.Sold ? "SOLD" : $"BUY   {offer.Cost}";
                card.Group.alpha = offer.Sold ? 0.4f : 1f;
            }
        }

        private void EnsureCards(int count)
        {
            while (cards.Count < count) cards.Add(MakeCard(cards.Count));

            const float w = 310f, gap = 28f;
            float total = cards.Count * w + (cards.Count - 1) * gap;
            cardBase.Clear();
            for (int i = 0; i < cards.Count; i++)
            {
                var pos = new Vector2(-total / 2f + w / 2f + i * (w + gap), 0f);
                cards[i].Root.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(w, 440f));
                cardBase.Add(pos);
            }
        }

        // ---------- Layout ----------
        private void Build()
        {
            var root = (RectTransform)UIFactory.Canvas("Shop Canvas", transform, 10).transform;
            var bg = UIFactory.Panel(root, new Color(0.02f, 0.01f, 0.05f, 0.88f), "ShopOverlay");
            bg.rectTransform.Fill();
            bg.raycastTarget = true;   // blocks clicks reaching the game
            group = bg.gameObject.AddComponent<CanvasGroup>();

            titleText = UIFactory.Label(bg.transform, "SOUL SHOP", 78, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            titleText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1200, 100));
            titleText.gameObject.AddComponent<GlitchText>().burstChance = 0.5f;
            var sub = UIFactory.Label(bg.transform, "Spend souls on perks. Press 1-5 or click to buy.", 26, TextAnchor.MiddleCenter, new Color(1, 1, 1, 0.55f));
            sub.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1200, 40));
            soulsText = UIFactory.Label(bg.transform, "0 SOULS", 44, TextAnchor.MiddleCenter, new Color(0.78f, 0.5f, 1f), FontStyle.Bold);
            soulsText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -205), new Vector2(800, 60));

            cardRow = UIFactory.Rect("Cards", bg.transform);
            cardRow.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1800, 460));

            rerollButton = UIFactory.Button(bg.transform, "REROLL", new Color(0.28f, 0.18f, 0.45f), () => shop.TryReroll(), out rerollText);
            ((RectTransform)rerollButton.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-200, 70), new Vector2(340, 80));

            var next = UIFactory.Button(bg.transform, "NEXT WAVE  >", new Color(0.1f, 0.45f, 0.55f), () => shop.Close(), out _, 32);
            ((RectTransform)next.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(200, 70), new Vector2(340, 80));

            Hide();
        }

        private Card MakeCard(int index)
        {
            var c = new Card();
            // Wrapper handles the fly-in alpha; the card itself handles sold-dimming and hover.
            var holder = UIFactory.Rect($"Card{index}", cardRow);
            c.Root = holder;
            c.Intro = holder.gameObject.AddComponent<CanvasGroup>();

            c.Background = UIFactory.Panel(holder, new Color(0.07f, 0.05f, 0.13f, 0.97f), "Body");
            if (UIFactory.Theme.cardMaterial != null) c.Background.material = new Material(UIFactory.Theme.cardMaterial);
            c.Background.rectTransform.Fill();
            c.Background.raycastTarget = true;
            c.Group = c.Background.gameObject.AddComponent<CanvasGroup>();
            c.Background.gameObject.AddComponent<UIButtonFx>().hoverScale = 1.04f;

            c.RarityBar = UIFactory.StyledPanel(c.Background.transform, Color.white, new Color(1, 1, 1, 0), "RarityBar", 4f, true);
            c.RarityBar.rectTransform.anchorMin = new Vector2(0.08f, 1);
            c.RarityBar.rectTransform.anchorMax = new Vector2(0.92f, 1);
            c.RarityBar.rectTransform.pivot = new Vector2(0.5f, 1);
            c.RarityBar.rectTransform.sizeDelta = new Vector2(0, 8);
            c.RarityBar.rectTransform.anchoredPosition = new Vector2(0, -10);
            var parent = c.Background.rectTransform;

            c.Category = UIFactory.Label(parent, "", 17, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            c.Category.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -26), new Vector2(200, 26));
            c.Rarity = UIFactory.Label(parent, "", 17, TextAnchor.UpperRight, Color.white, FontStyle.Bold);
            c.Rarity.rectTransform.Place(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-22, -26), new Vector2(160, 26));

            c.Name = UIFactory.Label(parent, "", 34, TextAnchor.UpperLeft, Color.white, FontStyle.Bold);
            c.Name.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -64), new Vector2(266, 90));

            c.Description = UIFactory.Label(parent, "", 23, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.75f));
            c.Description.rectTransform.Place(new Vector2(0, 1), new Vector2(0, 1), new Vector2(22, -160), new Vector2(266, 150));

            c.Owned = UIFactory.Label(parent, "", 20, TextAnchor.LowerLeft, new Color(1, 1, 1, 0.45f));
            c.Owned.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(22, 100), new Vector2(200, 26));
            c.Hotkey = UIFactory.Label(parent, "", 20, TextAnchor.LowerRight, new Color(1, 1, 1, 0.45f));
            c.Hotkey.rectTransform.Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-22, 100), new Vector2(80, 26));

            int captured = index;
            c.Buy = UIFactory.Button(parent, "BUY", new Color(0.35f, 0.2f, 0.55f), () => shop.TryBuy(captured), out c.ButtonText);
            ((RectTransform)c.Buy.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(266, 64));
            return c;
        }
    }
}
