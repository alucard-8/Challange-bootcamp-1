using System;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Small helpers to build uGUI from code, so views don't need hand-made prefabs.
    public static class UIFactory
    {
        private static Font font;
        private static UITheme theme;

        public static UITheme Theme
        {
            get
            {
                if (theme == null) theme = Resources.Load<UITheme>("UITheme");
                if (theme == null) theme = ScriptableObject.CreateInstance<UITheme>();
                return theme;
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

        // A themed panel: rounded corners + glowing animated border (UIPanel shader).
        public static Image StyledPanel(Transform parent, Color fill, Color? border = null, string name = "Panel",
                                        float radius = 14f, bool uniqueMaterial = false)
        {
            var img = Panel(parent, fill, name);
            var mat = Theme.panelMaterial;
            if (mat == null) return img;

            if (uniqueMaterial || border.HasValue || radius != 14f)
            {
                mat = new Material(mat);
                if (border.HasValue) mat.SetColor("_BorderColor", border.Value);
                mat.SetFloat("_Radius", radius);
            }
            img.material = mat;
            return img;
        }

        public static Canvas Canvas(string name, Transform parent, int sortOrder)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        // anchor = where on the parent, pivot = which point of this rect sits there.
        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Fill(this RectTransform rt, float padding = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
            return rt;
        }

        public static Image Panel(Transform parent, Color color, string name = "Panel")
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, TextAnchor align, Color color,
                                 FontStyle style = FontStyle.Normal, string name = "Label")
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
            return t;
        }

        public static Button Button(Transform parent, string label, Color color, Action onClick, out Text text, int fontSize = 28)
        {
            var img = StyledPanel(parent, color, Color.Lerp(color, Color.white, 0.55f), label + " Button", 12f);
            img.raycastTarget = true;
            img.gameObject.AddComponent<UIButtonFx>();

            var btn = img.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick());

            text = Label(img.transform, label, fontSize, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
            text.rectTransform.Fill();
            return btn;
        }

        // A horizontal bar; returns the fill rect. Use SetBar to change it.
        public static RectTransform Bar(Transform parent, Color back, Color fill, string name = "Bar")
        {
            var bg = Panel(parent, back, name);
            var f = Panel(bg.transform, fill, "Fill");
            f.rectTransform.Fill(2f);
            return f.rectTransform;
        }

        public static void SetBar(RectTransform fill, float value01)
        {
            fill.anchorMax = new Vector2(Mathf.Clamp01(value01), 1f);
            fill.offsetMax = new Vector2(-2f, -2f);
        }

        // Rebuilds a centered row of pip images when the count changes (ammo, shield charges).
        public static void Pips(RectTransform row, System.Collections.Generic.List<Image> pips, int count, float size, float gap,
                                Color color, float height = 0f)
        {
            if (pips.Count == count) return;
            foreach (var p in pips) UnityEngine.Object.Destroy(p.gameObject);
            pips.Clear();
            float h = height > 0f ? height : size;
            float total = count * size + (count - 1) * gap;
            for (int i = 0; i < count; i++)
            {
                var img = Panel(row, color, "Pip");
                img.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-total / 2f + size / 2f + i * (size + gap), 0f), new Vector2(size, h));
                pips.Add(img);
            }
        }

        public static string Hex(Color c) => ColorUtility.ToHtmlStringRGB(c);
    }
}
