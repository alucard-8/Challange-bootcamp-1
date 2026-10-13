using UnityEngine;

namespace Soulbreak.UI
{
    // Shared UI look (font, panel/card materials, colors). Lives in Resources so views can load it without wiring.
    [CreateAssetMenu(menuName = "Soulbreak/UI Theme", fileName = "UITheme")]
    public class UITheme : ScriptableObject
    {
        [Tooltip("OS font tried first (Windows ships Bahnschrift). Falls back to Unity's built-in font.")]
        public string osFontName = "Bahnschrift";
        public Material panelMaterial;
        public Material cardMaterial;
        public Material previewMaterial;

        [Header("Panels & buttons")]
        public Color panelColor = new Color(0.05f, 0.03f, 0.11f, 0.78f);
        public Color buttonColor = new Color(0.16f, 0.1f, 0.3f, 0.95f);
        public Color hudPanel = new Color(0.05f, 0.03f, 0.1f, 0.65f);
        public Color cardBackground = new Color(0.07f, 0.05f, 0.13f, 0.97f);
        public Color cardBackgroundSoft = new Color(0.07f, 0.05f, 0.13f, 0.95f);
        public Color cardBackgroundNear = new Color(0.06f, 0.04f, 0.11f, 0.95f);
        public Color cardSelectedBase = new Color(0.06f, 0.04f, 0.11f, 0.97f);
        public Color cardEquipped = new Color(0.12f, 0.08f, 0.22f, 0.97f);
        public Color buyBase = new Color(0.22f, 0.13f, 0.36f);
        public Color pauseDim = new Color(0.02f, 0f, 0.05f, 0.55f);
        public Color flashColor = new Color(1f, 0.95f, 0.85f);
        public Color transitionColor = new Color(0.85f, 0.92f, 1f);
        public Color screenFlash = new Color(1f, 0.95f, 1f);

        [Header("Accents")]
        public Color accent = new Color(0.45f, 0.85f, 1f);
        public Color soul = new Color(0.78f, 0.5f, 1f);
        public Color gold = new Color(1f, 0.82f, 0.35f);
        public Color goldSoft = new Color(1f, 0.9f, 0.6f);
        public Color bigHit = new Color(1f, 0.82f, 0.3f);
        public Color hudGold = new Color(1f, 0.8f, 0.3f);
        public Color lowAmmo = new Color(1f, 0.3f, 0.3f);
        public Color purple = new Color(0.85f, 0.4f, 1f);
        public Color good = new Color(0.45f, 1f, 0.6f);
        public Color bad = new Color(1f, 0.45f, 0.45f);
        public Color danger = new Color(1f, 0.35f, 0.4f);
        public Color blocked = new Color(0.5f, 0.85f, 1f);

        [Header("Text / dim")]
        public Color neutralText = new Color(1f, 1f, 1f, 0.7f);
        public Color dimText = new Color(1f, 1f, 1f, 0.5f);
        public Color dimTextStrong = new Color(1f, 1f, 1f, 0.6f);
        public Color faintText = new Color(1f, 1f, 1f, 0.4f);
        public Color dim = new Color(1f, 1f, 1f, 0.15f);

        [Header("Boss")]
        public Color bossPink = new Color(1f, 0.35f, 0.75f);
        public Color bossRed = new Color(1f, 0.2f, 0.2f);
        public Color gameOverBackdrop = new Color(0.03f, 0f, 0.04f);
        public Color stampGlow = new Color(1f, 0.95f, 0.6f);

        [Header("Leaderboard")]
        public Color rowPlayerBg = new Color(1f, 0.8f, 0.3f, 0.15f);
        public Color rowAlt = new Color(1f, 1f, 1f, 0.04f);
        public Color rowDimText = new Color(1f, 1f, 1f, 0.3f);

        [Header("Tutorial")]
        public Color keyCapBg = new Color(0.08f, 0.05f, 0.15f, 0.95f);
        public Color keyCapDone = new Color(0.2f, 0.6f, 0.35f, 0.95f);
        public Color confettiGold = new Color(1f, 0.79f, 0.3f);

        [Header("Shop / Index rarity")]
        public Color rarityCommon = new Color(0.65f, 0.72f, 0.82f);
        public Color rarityRare = new Color(0.3f, 0.65f, 1f);
        public Color rarityEpic = new Color(0.75f, 0.4f, 1f);
        public Color rarityLegendary = new Color(1f, 0.75f, 0.2f);

        [Header("Shop / Index category")]
        public Color catOffense = new Color(1f, 0.45f, 0.4f);
        public Color catEconomy = new Color(0.8f, 0.55f, 1f);
        public Color catDefense = new Color(0.4f, 0.85f, 1f);
        public Color catAbility = new Color(1f, 0.85f, 0.35f);
        public Color catWeapon = new Color(0.45f, 1f, 0.6f);

        // Rarity palette (Common, Rare, Epic, Legendary) for shader/card lookups.
        public Color Rarity(int i) => i switch
        {
            1 => rarityRare,
            2 => rarityEpic,
            3 => rarityLegendary,
            _ => rarityCommon,
        };

        // Category palette (Offense, Economy, Defense, Ability, Weapon) for shader/card lookups.
        public Color Category(int i) => i switch
        {
            1 => catEconomy,
            2 => catDefense,
            3 => catAbility,
            4 => catWeapon,
            _ => catOffense,
        };
    }
}
