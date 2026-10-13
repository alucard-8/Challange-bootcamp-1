using System.Collections.Generic;
using Soulbreak.GameFlow;
using Soulbreak.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Slide-out panel on the right edge (main menu + pause menu). Click the arrow tab to open it:
    // your best and last run (local), your global rank, and the global top 10.
    // In the game scene it only shows while paused.
    public class LeaderboardPanel : MonoBehaviour
    {
        [System.Serializable]
        private class Row
        {
            public Image background;
            public Text rank, name, score;
        }

        [SerializeField] private PauseController pause;   // optional: visible only while paused
        [SerializeField] private int sortingOrder = 12;

        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform slider;
        [SerializeField] private Button tabButton, refreshButton;
        [SerializeField] private Text arrowText, playerText, bestText, lastText, rankText, statusText;
        [SerializeField] private List<Row> rows = new List<Row>();
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private bool open, loading;
        private float slide;

        private void Awake()
        {
            canvas.sortingOrder = sortingOrder;
            tabButton.onClick.AddListener(Toggle);
            refreshButton.onClick.AddListener(() => _ = Refresh());
        }

        private void Start()
        {
            ShowLocal();
            ShowRows(new List<LeaderboardService.Entry>());
            _ = LeaderboardService.EnsureConnected();
        }

        private void Toggle()
        {
            open = !open;
            Audio.UIAudio.Play(open ? Audio.Sfx.UIOpen : Audio.Sfx.UIClose);
            if (open) _ = Refresh();
        }

        private void Update()
        {
            bool visible = pause == null || pause.IsPaused;
            group.alpha = Mathf.MoveTowards(group.alpha, visible ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = visible;
            if (!visible) open = false;

            slide = Mathf.MoveTowards(slide, open ? 1f : 0f, Time.unscaledDeltaTime * 4f);
            float e = 1f - Mathf.Pow(1f - slide, 3f);
            slider.anchoredPosition = new Vector2(Mathf.Lerp(slider.sizeDelta.x, 0f, e), slider.anchoredPosition.y);
            arrowText.text = open ? S.leaderboardArrowOpen : S.leaderboardArrowClosed;

            if (loading) statusText.text = S.leaderboardConnecting + new string('.', 1 + (int)(Time.unscaledTime * 3f) % 3);
        }

        private void ShowLocal()
        {
            string name = LeaderboardService.DisplayName;
            playerText.text = (string.IsNullOrEmpty(name) || name == S.leaderboardUnknownName ? S.leaderboardYou : name.ToUpper())
                + (AccountService.IsGuest ? S.leaderboardGuestTag : "");
            bestText.text = $"{MetaProgress.BestScore:N0}";
            lastText.text = $"{MetaProgress.LastScore:N0}";
        }

        private async System.Threading.Tasks.Task Refresh()
        {
            if (loading) return;
            loading = true;
            ShowLocal();
            var (top, me) = await LeaderboardService.Fetch();
            loading = false;
            if (this == null) return;   // left the scene while waiting

            ShowLocal();
            bool online = LeaderboardService.Online;
            rankText.text = me.HasValue ? $"#{me.Value.Rank}" : "-";
            statusText.text = online ? (top.Count == 0 ? S.leaderboardNoScores : "") : S.leaderboardOffline;
            ShowRows(top);
        }

        private void ShowRows(List<LeaderboardService.Entry> top)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                bool has = i < top.Count;
                var e = has ? top[i] : default;
                row.rank.text = has ? $"{e.Rank}" : $"{i + 1}";
                row.name.text = has ? e.Name : S.leaderboardEmptyName;
                row.score.text = has ? $"{e.Score:N0}" : "";
                var c = !has ? T.rowDimText : e.IsPlayer ? T.gold : i == 0 ? T.goldSoft : Color.white;
                row.rank.color = row.name.color = row.score.color = c;
                row.background.color = has && e.IsPlayer ? T.rowPlayerBg : (i % 2 == 0 ? T.rowAlt : new Color(1f, 1f, 1f, 0f));
            }
        }
    }
}
