using Soulbreak.Enemies;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Big boss health bar across the top while a boss is alive. Turns red when it enrages.
    public class BossHealthView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private EnemyEventChannel spawned;
        [SerializeField] private EnemyEventChannel bossEnraged;

        private Enemy boss;
        private CanvasGroup group;
        private RectTransform fill;
        private Image fillImage;
        private Text label;
        private float shown;

        private void Awake()
        {
            var panel = UIFactory.Rect("BossBar", hud.Root).Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(900, 70));
            group = panel.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            label = UIFactory.Label(panel, "", 28, TextAnchor.UpperCenter, Color.white, FontStyle.Bold);
            label.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(900, 34));
            fill = UIFactory.Bar(panel, new Color(0, 0, 0, 0.7f), new Color(1f, 0.35f, 0.75f), "Health");
            fill.parent.GetComponent<RectTransform>().Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -38), new Vector2(880, 22));
            fillImage = fill.GetComponent<Image>();
        }

        private void OnEnable()
        {
            spawned.OnRaised += HandleSpawned;
            bossEnraged.OnRaised += HandleEnraged;
        }

        private void OnDisable()
        {
            spawned.OnRaised -= HandleSpawned;
            bossEnraged.OnRaised -= HandleEnraged;
        }

        private void HandleSpawned(Enemy e)
        {
            if (!e.Config.isBoss) return;
            boss = e;
            shown = 1f;
            fillImage.color = new Color(1f, 0.35f, 0.75f);
            string n = string.IsNullOrEmpty(e.Config.displayName) ? e.Config.name : e.Config.displayName;
            label.text = n.ToUpper();
        }

        private void HandleEnraged(Enemy e)
        {
            if (e != boss) return;
            fillImage.color = new Color(1f, 0.2f, 0.2f);
            label.text += "  -  ENRAGED";
        }

        private void Update()
        {
            bool alive = boss != null && boss.IsAlive;
            group.alpha = Mathf.MoveTowards(group.alpha, alive ? 1f : 0f, Time.unscaledDeltaTime * 3f);
            if (!alive) return;
            float target = (float)boss.Health / boss.MaxHealth;
            shown = Mathf.Lerp(shown, target, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            UIFactory.SetBar(fill, shown);
        }
    }
}
