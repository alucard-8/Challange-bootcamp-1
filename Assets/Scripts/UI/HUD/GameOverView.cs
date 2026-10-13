using System.Collections.Generic;
using Soulbreak.Audio;
using Soulbreak.GameFlow;
using Soulbreak.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Cinematic game-over: the screen darkens, the title slams in, then a run summary tallies up
    // line by line, the score counts up, a NEW BEST stamp lands, essence is shown and the buttons appear.
    // Click anywhere to skip the tally. Driven entirely by the MetaRecorder's RunResult event.
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private HUDCanvas hud;
        [SerializeField] private GameManager game;
        [SerializeField] private MetaRecorder recorder;

        private const float RowsStart = 1.3f, RowGap = 0.22f, RowCount = 0.35f;

        [System.Serializable]
        private class Row
        {
            public Text label, value;
            public CanvasGroup group;
            [System.NonSerialized] public float target, start;
            [System.NonSerialized] public string format;
            [System.NonSerialized] public bool ticked;
        }

        private const float ScoreCount = 1.6f;

        [SerializeField] private CanvasGroup root, titleGroup, panelGroup, buttonsGroup, stampGroup, essenceGroup, bestGroup;
        private bool bestCrossed;
        private float badgeTime = -1f;
        [SerializeField] private Image backdrop;
        [SerializeField] private Text title, subtitle, scoreText, stampText, essenceText, bestText;
        [SerializeField] private RectTransform titleRt, panelRt, stampRt;
        [SerializeField] private Button skipButton, retryButton, menuButton;
        [SerializeField] private UIConfetti confetti;
        [SerializeField] private List<Row> rows = new List<Row>();
        [SerializeField] private UIStrings strings;
        [SerializeField] private UITheme theme;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;
        private UITheme T => theme != null ? theme : UIFactory.Theme;

        private RunResult result;
        private bool running;
        private float time, scoreStart, essenceAt, buttonsAt;
        private bool scoreTicked;

        private void Awake()
        {
            skipButton.onClick.AddListener(Skip);
            retryButton.onClick.AddListener(game.Restart);
            menuButton.onClick.AddListener(game.ToMainMenu);
        }
        private void OnEnable() => recorder.OnRunRecorded += Begin;
        private void OnDisable() => recorder.OnRunRecorded -= Begin;

        private void Begin(RunResult r)
        {
            result = r;
            running = true;
            time = 0f;
            root.alpha = 1f;
            root.blocksRaycasts = true;
            UIAudio.Play(Sfx.Collapse, 0.9f);

            var diff = DifficultyTable.Get(r.Difficulty);
            title.text = Lex.T("gameover", S.gameOverTitle);
            subtitle.text = string.Format(S.gameOverSubtitle, UIFactory.Hex(diff.Color), diff.Name, Mathf.Max(1, r.Wave));

            SetRow(0, Mathf.Max(1, r.Wave), "{0:0}");
            SetRow(1, r.Time, "{0:0}s");
            SetRow(2, r.Kills, "{0:0}");
            SetRow(3, r.EliteKills + r.BossKills, "{0:0}");
            SetRow(4, r.Headshots, "{0:0}");
            SetRow(5, r.BestStreak, "{0:0}");
            SetRow(6, r.SoulsEarned, "{0:0}");

            scoreStart = RowsStart + rows.Count * RowGap + 0.3f;
            essenceAt = scoreStart + ScoreCount + 0.4f;
            buttonsAt = essenceAt + 0.4f;
            scoreTicked = bestCrossed = false;
            badgeTime = -1f;
            stampGroup.alpha = 0f;

            // Show the record to beat; it disappears the moment the count passes it.
            bestText.text = string.Format(S.gameOverBest, (r.NewBest ? r.PreviousBest : MetaProgress.BestScore).ToString("N0"));
            bestGroup.alpha = 1f;
            essenceText.text = string.Format(S.gameOverEssence, r.Essence, Lex.T("essence", "SOUL ESSENCE"), MetaProgress.Essence);
        }

        // Debug preview (F3): plays the full game-over sequence with a fake run that beats your best.
        // Nothing is saved — your real records are untouched.
        public void PreviewNewBest()
        {
            int best = MetaProgress.BestScore;
            Begin(new RunResult
            {
                Score = best + Random.Range(300, 900), PreviousBest = best, NewBest = true,
                Wave = 7, WavesCleared = 6, Time = 214f, Kills = 128, EliteKills = 9, BossKills = 1,
                Headshots = 34, BestStreak = 27, SoulsEarned = 1840, Essence = 220,
                Difficulty = MetaProgress.SelectedDifficulty,
            });
        }

        private void SetRow(int i, float value, string format)
        {
            rows[i].target = value;
            rows[i].format = format;
            rows[i].start = RowsStart + i * RowGap;
            rows[i].ticked = false;
        }

        // Click anywhere: jump to the end of the sequence.
        private void Skip()
        {
            if (running && time < buttonsAt) time = buttonsAt;
        }

        private void Update()
        {
            if (!running) return;
            time += Time.unscaledDeltaTime;

            backdrop.color = new Color(T.gameOverBackdrop.r, T.gameOverBackdrop.g, T.gameOverBackdrop.b, Mathf.Clamp01(time / 1.0f) * 0.9f);

            // Title slam
            float tk = Mathf.Clamp01((time - 0.5f) / 0.25f);
            titleGroup.alpha = tk;
            titleRt.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, 1f - (1f - tk) * (1f - tk));
            if (tk > 0f && tk < 1f) titleRt.anchoredPosition = new Vector2(Random.Range(-6f, 6f), -150f + Random.Range(-4f, 4f));
            else titleRt.anchoredPosition = new Vector2(0f, -150f);

            // Summary panel slides up
            float pk = Mathf.Clamp01((time - 1.0f) / 0.4f);
            panelGroup.alpha = pk;
            panelRt.anchoredPosition = new Vector2(0f, Mathf.Lerp(-120f, 0f, 1f - (1f - pk) * (1f - pk)) - 30f);

            // Rows tally one by one
            foreach (var r in rows)
            {
                float k = Mathf.Clamp01((time - r.start) / RowCount);
                r.group.alpha = Mathf.Clamp01((time - r.start) / 0.12f);
                r.value.text = string.Format(r.format, Mathf.Lerp(0f, r.target, k));
                if (!r.ticked && time >= r.start) { r.ticked = true; UIAudio.Play(Sfx.Tally, 0.5f); }
            }

            // Score count-up (eased, slows near the end so you watch it pass the record)
            float sk = Mathf.Clamp01((time - scoreStart) / ScoreCount);
            float eased = 1f - Mathf.Pow(1f - sk, 3f);
            int shown = Mathf.RoundToInt(Mathf.Lerp(0, result.Score, eased));
            scoreText.text = $"{shown:N0}";
            if (!scoreTicked && time >= scoreStart) { scoreTicked = true; UIAudio.Play(Sfx.Streak, 0.6f); }

            // Passing the old best: the BEST line vanishes, a gold BEST! badge slams in beside the score with confetti
            if (result.NewBest && !bestCrossed && time >= scoreStart && shown > result.PreviousBest) CrossBest();

            float flash = badgeTime >= 0f ? Mathf.Clamp01(1f - (time - badgeTime) / 0.5f) : 0f;
            scoreText.color = Color.Lerp(T.gold, Color.white, flash);
            scoreText.rectTransform.localScale = Vector3.one * (1f + 0.12f * Mathf.Sin(sk * Mathf.PI) + 0.25f * flash * flash);

            bestGroup.alpha = Mathf.MoveTowards(bestGroup.alpha, bestCrossed ? 0f : 1f, Time.unscaledDeltaTime * 4f);

            if (badgeTime >= 0f)
            {
                float bk = Mathf.Clamp01((time - badgeTime) / 0.22f);
                stampGroup.alpha = bk;
                float pulse = 1f + 0.05f * Mathf.Sin((time - badgeTime) * 5f);
                stampRt.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, 1f - (1f - bk) * (1f - bk)) * pulse;
                PlaceBadge();
            }

            essenceGroup.alpha = Mathf.Clamp01((time - essenceAt) / 0.3f);

            float btn = Mathf.Clamp01((time - buttonsAt) / 0.3f);
            buttonsGroup.alpha = btn;
            buttonsGroup.interactable = buttonsGroup.blocksRaycasts = btn >= 1f;
        }

        private void CrossBest()
        {
            bestCrossed = true;
            badgeTime = time;
            UIAudio.Play(Sfx.NewBest, 0.9f);
            PlaceBadge();
            Vector2 at = scoreText.rectTransform.anchoredPosition;
            UIConfetti.Burst(confetti, at, T.gold, 44, 700f);
            UIConfetti.Burst(confetti, stampRt.anchoredPosition, T.stampGlow, 24, 420f);
        }

        // Keep the badge just to the right of the (changing-width) score number.
        private void PlaceBadge()
        {
            float half = Mathf.Min(scoreText.preferredWidth, scoreText.rectTransform.rect.width) * 0.5f;
            var s = scoreText.rectTransform.anchoredPosition;
            stampRt.anchoredPosition = new Vector2(s.x + half + 10f, s.y + 72f);   // sticker on the top-right of the number
        }
    }
}
