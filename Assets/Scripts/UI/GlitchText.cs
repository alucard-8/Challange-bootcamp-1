using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    [RequireComponent(typeof(Text))]
    public class GlitchText : MonoBehaviour
    {
        public float burstChance = 1.2f;
        public float maxOffset = 10f;
        public Color colorA = new Color(1f, 0.2f, 0.5f, 0.75f);
        public Color colorB = new Color(0.2f, 0.9f, 1f, 0.75f);

        private Text main, ghostA, ghostB;
        private float burst;

        private void Start()
        {
            main = GetComponent<Text>();
            ghostA = MakeGhost(colorA);
            ghostB = MakeGhost(colorB);
        }

        private Text MakeGhost(Color c)
        {
            var go = Instantiate(gameObject, transform.parent);
            DestroyImmediate(go.GetComponent<GlitchText>());
            foreach (var s in go.GetComponents<Shadow>()) DestroyImmediate(s);
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) DestroyImmediate(cg);
            go.name = name + " Ghost";
            go.transform.SetSiblingIndex(transform.GetSiblingIndex());
            var t = go.GetComponent<Text>();
            t.color = c;
            return t;
        }

        private void Update()
        {
            if (ghostA == null) return;
            ghostA.text = ghostB.text = main.text;

            var group = GetComponent<CanvasGroup>();
            float fade = group != null ? group.alpha : 1f;
            ghostA.color = new Color(colorA.r, colorA.g, colorA.b, colorA.a * fade);
            ghostB.color = new Color(colorB.r, colorB.g, colorB.b, colorB.a * fade);

            if (Random.value < burstChance * Time.unscaledDeltaTime) burst = Random.Range(0.08f, 0.25f);
            burst -= Time.unscaledDeltaTime;
            float amt = burst > 0f ? maxOffset : 1.5f;

            var rt = (RectTransform)transform;
            ((RectTransform)ghostA.transform).anchoredPosition = rt.anchoredPosition + new Vector2(Random.Range(-amt, amt), Random.Range(-amt, amt) * 0.3f);
            ((RectTransform)ghostB.transform).anchoredPosition = rt.anchoredPosition + new Vector2(Random.Range(-amt, amt), Random.Range(-amt, amt) * 0.3f);
        }
    }
}
