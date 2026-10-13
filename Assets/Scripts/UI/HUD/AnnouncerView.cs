using System.Collections.Generic;
using Soulbreak.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class AnnouncerView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private AnnouncementEventChannel announcements;
        [SerializeField] private float showTime = 1.8f;

        private readonly Queue<Announcement> queue = new Queue<Announcement>();
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform rt;
        private float timer;

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
