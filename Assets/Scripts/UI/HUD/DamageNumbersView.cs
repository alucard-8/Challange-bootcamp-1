using System.Collections.Generic;
using Soulbreak.Core;
using Soulbreak.Enemies;
using Soulbreak.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class DamageNumbersView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private EnemyDamageEventChannel damaged;
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Text numberTemplate;
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private class Number { public Text text; public RectTransform rt; public Vector2 velocity; public float age, life; public Color color; }

        private readonly List<Number> active = new List<Number>();
        private readonly Stack<Number> pool = new Stack<Number>();
        private Camera cam;

        private void OnEnable() => damaged.OnRaised += HandleDamage;
        private void OnDisable() => damaged.OnRaised -= HandleDamage;

        private void HandleDamage(EnemyDamage hit)
        {
            if (!GameSettings.DamageNumbers) return;
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            bool big = !hit.Blocked && hit.Amount >= stats.Get(StatType.Damage) * 1.5f;
            string label = hit.Blocked ? S.hudBlock : hit.Amount + (hit.Killed ? S.hudKillSuffix : "");
            var color = hit.Blocked ? T.blocked : big ? T.bigHit : Color.white;
            int size = hit.Blocked ? 24 : big ? 44 : 30;

            Vector2 screen = cam.WorldToScreenPoint(hit.Position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(hud.Root, screen, null, out Vector2 local);

            var n = pool.Count > 0 ? pool.Pop() : Create();
            n.text.gameObject.SetActive(true);
            n.text.text = label;
            n.text.fontSize = size;
            n.color = color;
            n.rt.anchoredPosition = local + new Vector2(Random.Range(-18f, 18f), 20f);
            n.velocity = new Vector2(Random.Range(-60f, 60f), Random.Range(160f, 220f));
            n.age = 0f;
            n.life = big ? 0.9f : 0.65f;
            n.rt.localScale = Vector3.one * (big ? 1.5f : 1.2f);
            active.Add(n);
        }

        private Number Create()
        {
            var t = UIFactory.Spawn(numberTemplate, hud.Root);
            return new Number { text = t, rt = t.rectTransform };
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var n = active[i];
                n.age += dt;
                n.velocity.y -= 520f * dt;
                n.rt.anchoredPosition += n.velocity * dt;
                n.rt.localScale = Vector3.Lerp(n.rt.localScale, Vector3.one, 1f - Mathf.Exp(-14f * dt));
                float k = n.age / n.life;
                n.text.color = new Color(n.color.r, n.color.g, n.color.b, 1f - k * k);
                if (k < 1f) continue;
                n.text.gameObject.SetActive(false);
                active.RemoveAt(i);
                pool.Push(n);
            }
        }
    }
}
