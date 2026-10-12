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

        [System.Serializable]
        private class Card
        {
            public RectTransform Root;
            public Image Background, RarityBar;
            public Text Category, Name, Description, Synergy, Owned, Rarity, Hotkey, ButtonText, Stamp;
            public Button Buy;
            public Image BuyImage;
            public UIButtonFx Fx;
            [System.NonSerialized] public float Hover;
            public CanvasGroup Group, Intro;
        }

        private readonly List<Vector2> cardBase = new List<Vector2>();
        private float showTime;

        [SerializeField] private CanvasGroup group;
        [SerializeField] private Text titleText, soulsText, rerollText;
        [SerializeField] private Button rerollButton, nextButton;
        [SerializeField] private RectTransform cardRow;
        [SerializeField] private List<Card> cards = new List<Card>();   // one per possible offer, unused ones hidden

        private void Awake()
        {
            rerollButton.onClick.AddListener(() => shop.TryReroll());
            nextButton.onClick.AddListener(shop.Close);
            for (int i = 0; i < cards.Count; i++)
            {
                int index = i;
                cards[i].Buy.onClick.AddListener(() => shop.TryBuy(index));
                // each card animates its own shader values, so it needs its own material instance
                if (cards[i].Background.material != null) cards[i].Background.material = new Material(cards[i].Background.material);
            }
            Hide();
        }

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
            for (int i = 0; i < cards.Count && i < cardBase.Count; i++)
            {
                float t = Mathf.Clamp01((showTime - 0.12f - i * 0.07f) / 0.45f);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                cards[i].Intro.alpha = ease;
                // Hover: card lifts and the shader edge brightens.
                var c = cards[i];
                c.Hover = Mathf.MoveTowards(c.Hover, c.Fx.IsHovered ? 1f : 0f, Time.unscaledDeltaTime * 6f);
                float h = c.Hover * c.Hover * (3f - 2f * c.Hover);
                c.Root.anchoredPosition = cardBase[i] + Vector2.down * 120f * (1f - ease) + Vector2.up * 16f * h;
                var mat = c.Background.material;
                if (mat != null && mat.HasProperty("_Hover")) mat.SetFloat("_Hover", h);
            }
        }

        private void Show()
        {
            showTime = 0f;
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
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
            soulsText.text = $"{wallet.Souls} {Lex.T("souls", "SOULS")}";
            rerollText.text = $"REROLL  ({shop.RerollCost})";
            rerollButton.interactable = wallet.Souls >= shop.RerollCost;

            var offers = shop.Offers;
            LayoutCards(Mathf.Min(offers.Count, cards.Count));

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
                    var accent = technique != null && technique.module != null ? (Color)technique.module.color * 0.45f + rarity * 0.55f : rarity;
                    float peak = Mathf.Max(accent.r, Mathf.Max(accent.g, accent.b));
                    if (peak > 1f) accent /= peak;   // module colors are HDR; keep the card face readable
                    accent.a = 1f;
                    mat.SetColor("_Accent", accent);
                    mat.SetFloat("_Style", technique != null ? 1f : 0f);
                    mat.SetFloat("_Icon", technique != null && technique.module != null ? technique.module.icon : 0f);
                    mat.SetFloat("_Dim", offer.Sold ? 1f : 0f);
                    mat.SetFloat("_Rarity", (int)perk.rarity);
                    mat.SetFloat("_Category", (int)perk.category);
                }
                card.Rarity.text = perk.rarity.ToString().ToUpper();
                card.Rarity.color = rarity;
                card.Category.text = perk.IsTechnique ? "TECHNIQUE" : perk.category.ToString().ToUpper();
                card.Category.color = cat;
                card.Name.text = perk.displayName;
                var tech = perk.TechniqueEffect;
                card.Description.text = perk.description;
                card.Synergy.text = synergies != null && tech != null ? synergies.HintFor(tech.module).Trim() : "";
                card.Owned.text = perk.maxStacks > 0 ? $"OWNED {stacks}/{perk.maxStacks}" : (stacks > 0 ? $"OWNED {stacks}" : "");
                card.Hotkey.text = $"[{i + 1}]";

                card.Background.color = Color.Lerp(new Color(0.07f, 0.05f, 0.13f, 0.97f), rarity, 0.08f);
                card.Buy.interactable = !offer.Sold && affordable;
                card.BuyImage.color = Color.Lerp(new Color(0.22f, 0.13f, 0.36f), rarity, affordable ? 0.45f : 0.1f);
                card.ButtonText.text = offer.Sold ? "SOLD" : affordable ? $"BUY  -  {offer.Cost}" : $"NEED {offer.Cost}";
                card.ButtonText.color = offer.Sold || affordable ? Color.white : new Color(1f, 0.45f, 0.45f);
                card.Stamp.gameObject.SetActive(offer.Sold);
                card.Group.alpha = offer.Sold ? 0.55f : 1f;
            }
        }

        // Centers however many cards are on offer.
        private void LayoutCards(int count)
        {
            const float gap = 30f;
            float w = cards[0].Root.sizeDelta.x;
            float total = count * w + (count - 1) * gap;
            cardBase.Clear();
            for (int i = 0; i < cards.Count; i++)
                cardBase.Add(new Vector2(-total / 2f + w / 2f + i * (w + gap), 0f));
        }
    }
}
