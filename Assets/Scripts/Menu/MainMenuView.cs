using System.Collections.Generic;

using Soulbreak.GameFlow;
using Soulbreak.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.Menu
{
    // Main menu UI: glitching title, staggered intro animation, Play / How to play / Quit,
    // and a dive-into-the-reactor transition when the run starts.
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private MenuAmbience ambience;
        [SerializeField] private AscensionView ascension;

        private class Intro { public RectTransform rt; public CanvasGroup group; public Vector2 basePos, offset; public float delay; }

        private readonly List<Intro> intro = new List<Intro>();
        private CanvasGroup howTo, mainGroup;
        private RectTransform title;
        private Vector2 titleBase;
        private float time;
        private bool starting, howToOpen;

        private void Start()
        {
            Time.timeScale = 1f;
            Build();
            SceneFader.FadeFromBlack(1.4f);
        }

        private void Update()
        {
            time += Time.unscaledDeltaTime;

            foreach (var e in intro)
            {
                float t = Mathf.Clamp01((time - e.delay) / 0.7f);
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                e.group.alpha = ease;
                if (t < 1f) e.rt.anchoredPosition = e.basePos + e.offset * (1f - ease);
            }

            // Title gently floats once its intro is done.
            if (time > 1f) title.anchoredPosition = titleBase + Vector2.up * Mathf.Sin(time * 1.3f) * 6f;

            float h = Mathf.MoveTowards(howTo.alpha, howToOpen ? 1f : 0f, Time.unscaledDeltaTime * 5f);
            howTo.alpha = h;
            howTo.blocksRaycasts = howTo.interactable = howToOpen;
            mainGroup.alpha = 1f - h * 0.85f;

        }

        public bool IsHowToOpen => howToOpen;
        public bool IsAscensionOpen => ascension.IsOpen;
        public void CloseAscension() => ascension.Close();
        public void CloseHowTo() => howToOpen = false;

        public void Play()
        {
            if (starting) return;
            starting = true;
            mainGroup.interactable = false;
            ambience.ZoomIn();
            SceneFader.LoadScene(SceneNames.Game, new Color(0.85f, 0.92f, 1f), 0.75f);
        }

        // ---------- Layout ----------
        private void Build()
        {
            var theme = UIFactory.Theme;
            var root = (RectTransform)UIFactory.Canvas("Menu Canvas", transform, 0).transform;

            var main = UIFactory.Rect("Main", root);
            main.Fill();
            mainGroup = main.gameObject.AddComponent<CanvasGroup>();

            // Title
            var titleText = UIFactory.Label(main, "SOULBREAK", 190, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, "Title");
            title = titleText.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1600, 230));
            titleBase = title.anchoredPosition;
            var glow = titleText.gameObject.AddComponent<Outline>();
            glow.effectColor = new Color(theme.accent.r, theme.accent.g, theme.accent.b, 0.35f);
            glow.effectDistance = new Vector2(4, -4);
            titleText.gameObject.AddComponent<GlitchText>().maxOffset = 14f;
            AddIntro(title, 0.15f, new Vector2(0, 80));

            var tag = UIFactory.Label(main, "THE  REACTOR  MUST  NOT  FALL", 34, TextAnchor.MiddleCenter, theme.accent, FontStyle.Bold, "Tagline");
            AddIntro(tag.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(1200, 50)), 0.45f, new Vector2(0, 30));

            // Buttons
            string[] labels = { "PLAY", "ASCENSION", "HOW TO PLAY", "QUIT" };
            System.Action[] actions = { Play, () => ascension.Open(), () => howToOpen = true, GameManager.Quit };
            for (int i = 0; i < labels.Length; i++)
            {
                var color = i == 0 ? new Color(0.1f, 0.42f, 0.55f, 0.95f) : theme.buttonColor;
                var btn = UIFactory.Button(main, labels[i], color, actions[i], out _, i == 0 ? 44 : 32);
                var rt = ((RectTransform)btn.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                    new Vector2(0, 330 - i * 88), new Vector2(i == 0 ? 460 : 400, i == 0 ? 86 : 72));
                AddIntro(rt, 0.7f + i * 0.12f, new Vector2(0, -60));
            }

            var footL = UIFactory.Label(main, "Workshop 1  -  Observer Pattern Challenge", 20, TextAnchor.LowerLeft, new Color(1, 1, 1, 0.35f));
            AddIntro(footL.rectTransform.Place(new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 24), new Vector2(800, 30)), 1.2f, Vector2.zero);
            var footR = UIFactory.Label(main, "ENTER  to play", 20, TextAnchor.LowerRight, new Color(1, 1, 1, 0.35f));
            AddIntro(footR.rectTransform.Place(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 24), new Vector2(400, 30)), 1.2f, Vector2.zero);

            BuildHowTo(root, theme);
        }

        private void BuildHowTo(RectTransform root, UITheme theme)
        {
            var panel = UIFactory.StyledPanel(root, new Color(0.04f, 0.02f, 0.09f, 0.94f), theme.accent, "HowTo", 24f);
            panel.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 760));
            panel.raycastTarget = true;
            howTo = panel.gameObject.AddComponent<CanvasGroup>();
            howTo.alpha = 0f;
            howTo.blocksRaycasts = false;

            var head = UIFactory.Label(panel.transform, "HOW TO PLAY", 64, TextAnchor.UpperCenter, Color.white, FontStyle.Bold);
            head.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(1000, 80));

            string soul = UIFactory.Hex(theme.soul), acc = UIFactory.Hex(theme.accent);
            var body = UIFactory.Label(panel.transform,
                $"<color=#{acc}><b>DEFEND</b></color>   Monsters swarm the soul reactor in waves. Click them before they reach it.\n\n" +
                $"<color=#{acc}><b>THE RING</b></color>   The slot your aim passes through decides your shot. Fired modules empty out and reload a new one.\n\n" +
                $"<color=#{soul}><b>SOULS</b></color>   Kills give souls. Shoot the orbs they drop for more - green orbs restore a shield charge.\n\n" +
                $"<color=#{soul}><b>SHOP</b></color>   Between waves, spend souls on perks and weapon modules. Reroll if you don't like the offers.\n\n" +
                "<b>LMB</b> shoot      <b>R / RMB</b> reload      <b>SPACE</b> nova      <b>ESC</b> pause",
                28, TextAnchor.UpperLeft, new Color(1, 1, 1, 0.85f));
            body.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(1060, 480));

            var back = UIFactory.Button(panel.transform, "BACK", theme.buttonColor, () => howToOpen = false, out _, 30);
            ((RectTransform)back.transform).Place(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(300, 72));
        }

        private void AddIntro(RectTransform rt, float delay, Vector2 offset)
        {
            var g = rt.gameObject.GetComponent<CanvasGroup>();
            if (g == null) g = rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            intro.Add(new Intro { rt = rt, group = g, basePos = rt.anchoredPosition, offset = offset, delay = delay });
            rt.anchoredPosition += offset;
        }
    }
}
