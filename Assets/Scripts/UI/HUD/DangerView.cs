using System.Collections.Generic;
using Soulbreak.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class DangerView : MonoBehaviour
    {
        private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");

        private static readonly Color CbEnemy = new Color(0.34f, 0.71f, 0.91f, 0.9f);
        private static readonly Color CbElite = new Color(0.94f, 0.89f, 0.26f, 0.9f);
        private static readonly Color CbBoss = new Color(0.90f, 0.62f, 0f, 0.9f);

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
        private Material vignetteInstance;

        private void Awake()
        {
            cam = Camera.main;

            if (vignetteMaterial != null && vignette != null)
            {
                vignetteInstance = Instantiate(vignetteMaterial);
                vignette.material = vignetteInstance;
            }
        }

        private void OnDestroy()
        {
            if (vignetteInstance != null) Destroy(vignetteInstance);
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
                arrow.color = ArrowColor(e);
                arrow.rectTransform.localScale = Vector3.one * (e.Config.isBoss ? 1.8f : e.IsElite ? 1.3f : 1f);
            }
            for (int i = used; i < arrows.Count; i++) arrows[i].enabled = false;

            float strength = Mathf.Max(danger, lowHealth);
            var vc = vignette.color;
            vc.a = Mathf.MoveTowards(vc.a, strength * 0.35f, Time.unscaledDeltaTime * 1.5f);
            vignette.color = vc;

            if (vignetteInstance != null)
                vignetteInstance.SetFloat(PulseSpeedId, Core.GameSettings.ReduceFlashing ? 0f : 2f + strength * 3f);
        }

        private static Color ArrowColor(Enemy e)
        {
            if (Core.GameSettings.Colorblind)
                return e.Config.isBoss ? CbBoss : e.IsElite ? CbElite : CbEnemy;

            var c = e.Config.color;
            float max = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            return new Color(c.r / max, c.g / max, c.b / max, 0.9f);
        }

        private Image GetArrow(int i)
        {
            while (arrows.Count <= i) arrows.Add(UIFactory.Spawn(arrowTemplate, hud.Root));
            arrows[i].enabled = true;
            return arrows[i];
        }
    }
}
