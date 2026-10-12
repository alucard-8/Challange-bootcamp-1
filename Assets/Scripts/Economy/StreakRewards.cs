using Soulbreak.Core.Events;
using UnityEngine;

namespace Soulbreak.Economy
{
    // Observer: pays bonus souls on kill-streak milestones and broadcasts the announcement.
    public class StreakRewards : MonoBehaviour
    {
        [SerializeField] private ComboSystem combo;
        [SerializeField] private SoulWallet wallet;
        [SerializeField] private Transform payoutPoint;
        [SerializeField] private AnnouncementEventChannel announcements;
        [SerializeField] private int soulsPerKillInStreak = 3;

        private void OnEnable() => combo.OnMilestone += HandleMilestone;
        private void OnDisable() => combo.OnMilestone -= HandleMilestone;

        private void HandleMilestone(int count, string title)
        {
            int bonus = count * soulsPerKillInStreak;
            wallet.Add(bonus, payoutPoint.position);
            announcements.Raise(new Announcement(title, $"{count} KILL STREAK   +{bonus} SOULS", new Color(1f, 0.75f, 0.25f)));
        }
    }
}
