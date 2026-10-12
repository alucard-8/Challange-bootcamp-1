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
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform fill;
        [SerializeField] private Image fillImage;
        [SerializeField] private Text label;
        private float shown;

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
