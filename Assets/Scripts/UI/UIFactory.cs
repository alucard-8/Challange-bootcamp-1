using System;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Runtime UI helpers shared by the views: theme/font lookup, bars, pips, template cloning, easing.
    // All layout lives in the UI prefabs (Assets/Prefabs/UI).
    public static class UIFactory
    {
        private static Font font;
        private static UITheme theme;
        private static UIStrings strings;

        public static UITheme Theme
        {
            get
            {
                if (theme == null) theme = Resources.Load<UITheme>("UITheme");
                if (theme == null) theme = ScriptableObject.CreateInstance<UITheme>();
                return theme;
            }
        }

        // Fallback copy source for views whose [SerializeField] ref is not wired.
        public static UIStrings Strings
        {
            get
            {
                if (strings == null) strings = Resources.Load<UIStrings>("UIStrings");
                if (strings == null) strings = ScriptableObject.CreateInstance<UIStrings>();
                return strings;
            }
        }

        public static Font Font
        {
            get
            {
                if (font != null) return font;
                string wanted = Theme.osFontName;
                if (!string.IsNullOrEmpty(wanted) && Array.IndexOf(Font.GetOSInstalledFontNames(), wanted) >= 0)
                    font = Font.CreateDynamicFontFromOSFont(wanted, 32);
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static void SetBar(RectTransform fill, float value01)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(value01), 1f);
            fill.offsetMax = new Vector2(-2f, -2f);
        }

        // Lays out a centered row of pips (ammo, shield charges) by cloning the row's inactive template.
        public static void Pips(Image template, System.Collections.Generic.List<Image> pips, int count, float gap)
        {
            if (pips.Count == count) return;
            while (pips.Count < count) pips.Add(Spawn(template, template.transform.parent));
            while (pips.Count > count)
            {
                UnityEngine.Object.Destroy(pips[pips.Count - 1].gameObject);
                pips.RemoveAt(pips.Count - 1);
            }
            float size = template.rectTransform.sizeDelta.x;
            float total = count * size + (count - 1) * gap;
            for (int i = 0; i < count; i++)
                pips[i].rectTransform.anchoredPosition = new Vector2(-total / 2f + size / 2f + i * (size + gap), 0f);
        }

        // Clones an inactive template (popups, pips, arrows) and switches the copy on.
        public static T Spawn<T>(T template, Transform parent) where T : Component
        {
            var copy = UnityEngine.Object.Instantiate(template, parent, false);
            copy.gameObject.SetActive(true);
            return copy;
        }

        // Panel "pop" scale for an open amount 0..1 (eases out with a small overshoot).
        public static float PopScale(float open01)
        {
            float t = Mathf.Clamp01(open01) - 1f;
            float back = 1f + 2.4f * t * t * t + 1.4f * t * t;
            return Mathf.Lerp(0.88f, 1f, back);
        }

        public static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
    }
}
