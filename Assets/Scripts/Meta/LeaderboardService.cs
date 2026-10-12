using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Global leaderboard on Unity Gaming Services. Every call fails soft: offline, the game keeps
    // working with local records only.
    public static class LeaderboardService
    {
        // Create a leaderboard with this ID in the Unity Cloud dashboard (sort: high to low, keep best).
        public const string LeaderboardId = "soulbreak_highscores";
        public const int TopCount = 10;

        public struct Entry
        {
            public int Rank;          // 1-based
            public string Name;
            public int Score;
            public bool IsPlayer;
        }

        public static bool Online => AccountService.Status == AccountService.State.Online;
        public static string PlayerName { get; private set; } = "";

        // Sign-in (guest or account) is owned by AccountService.
        public static async Task<bool> EnsureConnected()
        {
            if (!await AccountService.EnsureSignedIn()) return false;
            try { PlayerName = await AuthenticationService.Instance.GetPlayerNameAsync(); } catch (Exception) { }
            return true;
        }

        public static async Task SubmitScore(int score)
        {
            if (score <= 0 || !await EnsureConnected()) return;
            try { await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score); }
            catch (Exception e) { Debug.LogWarning($"Leaderboard post failed: {e.Message}"); }
        }

        // Top entries plus the player's own entry (null if they haven't posted a score yet).
        public static async Task<(List<Entry> top, Entry? player)> Fetch()
        {
            var top = new List<Entry>();
            Entry? me = null;
            if (!await EnsureConnected()) return (top, null);
            try
            {
                string myId = AuthenticationService.Instance.PlayerId;
                var page = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId,
                    new Unity.Services.Leaderboards.GetScoresOptions { Limit = TopCount });
                foreach (var r in page.Results)
                    top.Add(new Entry { Rank = r.Rank + 1, Name = Clean(r.PlayerName), Score = (int)r.Score, IsPlayer = r.PlayerId == myId });

                try
                {
                    var mine = await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
                    me = new Entry { Rank = mine.Rank + 1, Name = Clean(mine.PlayerName), Score = (int)mine.Score, IsPlayer = true };
                }
                catch (Exception) { /* no score posted yet */ }
            }
            catch (Exception e) { Debug.LogWarning($"Leaderboard fetch failed: {e.Message}"); }
            return (top, me);
        }

        // "Swift#4821" -> "Swift"
        private static string Clean(string name)
        {
            if (string.IsNullOrEmpty(name)) return "???";
            int hash = name.LastIndexOf('#');
            return hash > 0 ? name.Substring(0, hash) : name;
        }

        // The account username if logged in, else the auto-generated guest name.
        public static string DisplayName => !AccountService.IsGuest ? AccountService.Username : Clean(PlayerName);
    }
}
