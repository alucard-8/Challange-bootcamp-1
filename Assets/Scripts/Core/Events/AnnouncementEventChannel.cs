using UnityEngine;

namespace Soulbreak.Core.Events
{
    [CreateAssetMenu(menuName = "Soulbreak/Events/Announcement Event Channel", fileName = "AnnouncementEventChannel")]
    public class AnnouncementEventChannel : EventChannel<Announcement> { }

    [System.Serializable]
    public struct Announcement
    {
        public string Title;
        public string Subtitle;
        public Color Color;

        public Announcement(string title, string subtitle, Color color)
        {
            Title = title;
            Subtitle = subtitle;
            Color = color;
        }
    }
}
