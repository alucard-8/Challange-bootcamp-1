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
        public Color panelColor = new Color(0.05f, 0.03f, 0.11f, 0.78f);
        public Color buttonColor = new Color(0.16f, 0.1f, 0.3f, 0.95f);
        public Color accent = new Color(0.45f, 0.85f, 1f);
        public Color soul = new Color(0.78f, 0.5f, 1f);
    }
}
