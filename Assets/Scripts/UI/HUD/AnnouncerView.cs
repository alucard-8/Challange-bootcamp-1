using System.Collections.Generic;
using Soulbreak.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Arena-announcer style callouts (kill streaks, synergies, map events, boss phases).
    // Listens to one announcement channel; queues messages so they don't overlap.
    public class AnnouncerView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private AnnouncementEventChannel announcements;
        [SerializeField] private float showTime = 1.8f;

        private readonly Queue<Announcement> queue = new Queue<Announcement>();
        private Text title, subtitle;
        private CanvasGroup group;
        private RectTransform rt;
        private float timer;

        private void Awake()
        {
            rt = UIFactory.Rect("Announcer", hud.Root).Place(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -230), new Vector2(1400, 170));
            group = rt.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            title = UIFactory.Label(rt, "", 78, TextAnchor.UpperCenter, Color.white, FontStyle.Bold, "Title");
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(1400, 100));
            title.gameObject.AddComponent<GlitchText>().burstChance = 3f;
            subtitle = UIFactory.Label(rt, "", 28, TextAnchor.UpperCenter, new Color(1, 1, 1, 0.8f), FontStyle.Bold, "Subtitle");
            subtitle.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(1400, 50));
        }

        private void OnEnable() => announcements.OnRaised += Enqueue;
        private void OnDisable() => announcements.OnRaised -= Enqueue;

        private void Enqueue(Announcement a) => queue.Enqueue(a);

        private void Update()
        {
            if (timer > 0f)
            {
                timer -= Time.unscaledDeltaTime;
                float age = showTime - timer;
                float pop = Mathf.Clamp01(age / 0.15f);
                rt.localScale = Vector3.one * Mathf.Lerp(1.6f, 1f, 1f - (1f - pop) * (1f - pop));
                group.alpha = Mathf.Min(pop, Mathf.Clamp01(timer / 0.4f));
                return;
            }

            if (queue.Count == 0) { group.alpha = 0f; return; }
            var next = queue.Dequeue();
            title.text = next.Title;
            title.color = next.Color;
            subtitle.text = next.Subtitle;
            timer = showTime;
        }
    }
}
