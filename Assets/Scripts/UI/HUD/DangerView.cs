using System.Collections.Generic;
using Soulbreak.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class DangerView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private EnemyRegistry registry;
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private Material vignetteMaterial;
        [SerializeField] private float edgeMargin = 46f;

        private readonly List<Image> arrows = new List<Image>();
        [SerializeField] private Image vignette;
        [SerializeField] private Image arrowTemplate;
        private float lowHealth;
        private Camera cam;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void OnEnable() => reactor.OnHealthChanged += HandleHealth;
        private void OnDisable() => reactor.OnHealthChanged -= HandleHealth;
        private void HandleHealth(int cur, int max) => lowHealth = max > 0 ? Mathf.Clamp01(1f - (float)cur / max / 0.3f) * 0.8f : 0f;

        private void LateUpdate()
        {
            if (cam == null) cam = Camera.main;
            int used = 0;
            float danger = 0f;
            Vector2 size = hud.Root.rect.size;

            foreach (var e in registry.Alive)
            {
                if (e == null) continue;
                float dist = Vector2.Distance(e.transform.position, reactor.transform.position);
                danger = Mathf.Max(danger, Mathf.Clamp01((2.6f - dist) / 1.4f));

                Vector3 vp = cam.WorldToViewportPoint(e.transform.position);
                bool onScreen = vp.x > 0f && vp.x < 1f && vp.y > 0f && vp.y < 1f;
                if (onScreen) continue;

                var arrow = GetArrow(used++);
                Vector2 dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
                float scale = Mathf.Min((size.x * 0.5f - edgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.x * size.x)),
                                        (size.y * 0.5f - edgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.y * size.y)));
                Vector2 pos = new Vector2(dir.x * size.x, dir.y * size.y) * scale;
                arrow.rectTransform.anchoredPosition = pos;
                arrow.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 45f);
                var c = e.Config.color;
                float max = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
                arrow.color = new Color(c.r / max, c.g / max, c.b / max, 0.9f);
                arrow.rectTransform.localScale = Vector3.one * (e.Config.isBoss ? 1.8f : e.IsElite ? 1.3f : 1f);
            }
            for (int i = used; i < arrows.Count; i++) arrows[i].enabled = false;

            float strength = Mathf.Max(danger, lowHealth);
            var vc = vignette.color;
            vc.a = Mathf.MoveTowards(vc.a, strength * 0.35f, Time.unscaledDeltaTime * 1.5f);
            vignette.color = vc;
            if (vignette.material != null) vignette.material.SetFloat("_PulseSpeed", 2f + strength * 3f);
        }

        private Image GetArrow(int i)
        {
            while (arrows.Count <= i) arrows.Add(UIFactory.Spawn(arrowTemplate, hud.Root));
            arrows[i].enabled = true;
            return arrows[i];
        }
    }
}
