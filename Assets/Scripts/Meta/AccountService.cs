using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Player accounts on Unity Gaming Services.
    //  - Everyone starts as a guest (anonymous sign-in, remembered by the browser).
    //  - Create account: upgrades the current guest to username + password, keeping its scores and progress.
    //  - Log in: switches to an existing account and loads its progress from Cloud Save.
    // Progress (MetaProgress) is pushed to Cloud Save whenever it changes, so it follows the account anywhere.
    public static class AccountService
    {
        public enum State { Idle, Connecting, Online, Offline }

        private const string UserKey = "soulbreak.account.user";
        private const string CloudKey = "progress";

        public static State Status { get; private set; } = State.Idle;
        public static string Error { get; private set; } = "";
        public static string Username => PlayerPrefs.GetString(UserKey, "");
        public static bool IsGuest => string.IsNullOrEmpty(Username);
        public static string PlayerId => Status == State.Online ? AuthenticationService.Instance.PlayerId : "";

        public static event Action OnAccountChanged;

        private static Task<bool> connecting;
        private static bool syncing, pushQueued;

        // Safe to call any number of times; signs in once (resuming the last session if there is one).
        public static Task<bool> EnsureSignedIn()
        {
            if (Status == State.Online) return Task.FromResult(true);
            if (connecting == null || (connecting.IsCompleted && Status == State.Offline)) connecting = Connect();
            return connecting;
        }

        private static async Task<bool> Connect()
        {
            Status = State.Connecting;
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized) await UnityServices.InitializeAsync();
                if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Status = State.Online;
                await SyncProgress(preferCloud: false);
                MetaProgress.OnChanged -= QueuePush;
                MetaProgress.OnChanged += QueuePush;
                return true;
            }
            catch (Exception e)
            {
                Fail(e);
                return false;
            }
        }

        // Turn the current guest into an account (keeps scores + progress).
        public static async Task<string> CreateAccount(string username, string password)
        {
            string invalid = Validate(username, password);
            if (invalid != null) return invalid;
            if (!await EnsureSignedIn()) return "Can't reach the server. Check your connection.";
            try
            {
                if (IsGuest) await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
                else
                {
                    AuthenticationService.Instance.SignOut(true);
                    await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                }
                await SetUser(username);
                await PushProgress();
                return null;
            }
            catch (Exception e) { return Friendly(e); }
        }

        // Switch to an existing account and load its progress.
        public static async Task<string> LogIn(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password)) return "Enter your username and password.";
            if (!await EnsureSignedIn()) return "Can't reach the server. Check your connection.";
            try
            {
                AuthenticationService.Instance.SignOut(true);
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username.Trim(), password);
                await SetUser(username.Trim());
                await SyncProgress(preferCloud: true);
                return null;
            }
            catch (Exception e)
            {
                // fall back to a fresh guest so the game stays online
                PlayerPrefs.DeleteKey(UserKey);
                try { if (!AuthenticationService.Instance.IsSignedIn) await AuthenticationService.Instance.SignInAnonymouslyAsync(); } catch { }
                OnAccountChanged?.Invoke();
                return Friendly(e);
            }
        }

        // Back to a new guest. Local progress stays on this device.
        public static async Task LogOut()
        {
            if (Status != State.Online) return;
            try
            {
                AuthenticationService.Instance.SignOut(true);
                PlayerPrefs.DeleteKey(UserKey);
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await PushProgress();
            }
            catch (Exception e) { Fail(e); }
            OnAccountChanged?.Invoke();
        }

        private static async Task SetUser(string username)
        {
            PlayerPrefs.SetString(UserKey, username);
            PlayerPrefs.Save();
            try { await AuthenticationService.Instance.UpdatePlayerNameAsync(username); }   // shown on the leaderboard
            catch (Exception e) { Debug.LogWarning($"Couldn't set player name: {e.Message}"); }
            OnAccountChanged?.Invoke();
        }

        // ---------- Cloud Save ----------
        // preferCloud: after logging in the account's cloud copy wins; otherwise the newer copy wins.
        private static async Task SyncProgress(bool preferCloud)
        {
            if (syncing) return;
            syncing = true;
            try
            {
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { CloudKey });
                string cloudJson = result.TryGetValue(CloudKey, out var item) ? item.Value.GetAs<string>() : null;
                long cloudAt = 0;
                if (!string.IsNullOrEmpty(cloudJson))
                {
                    var probe = JsonUtility.FromJson<Stamp>(cloudJson);
                    cloudAt = probe != null ? probe.savedAt : 0;
                }

                bool useCloud = !string.IsNullOrEmpty(cloudJson) && (preferCloud || cloudAt > MetaProgress.SavedAt);
                syncing = false;
                if (useCloud) MetaProgress.ImportJson(cloudJson);
                else await PushProgress();
            }
            catch (Exception e) { Debug.LogWarning($"Cloud save sync failed: {e.Message}"); }
            finally { syncing = false; }
        }

        [Serializable] private class Stamp { public long savedAt; }

        private static void QueuePush()
        {
            if (syncing || pushQueued) return;
            pushQueued = true;
            _ = PushSoon();
        }

        private static async Task PushSoon()
        {
            await Task.Yield();   // batch several changes made in the same frame
            pushQueued = false;
            await PushProgress();
        }

        private static async Task PushProgress()
        {
            if (Status != State.Online || syncing) return;
            try { await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> { { CloudKey, MetaProgress.ExportJson() } }); }
            catch (Exception e) { Debug.LogWarning($"Cloud save failed: {e.Message}"); }
        }

        // ---------- helpers ----------
        private static string Validate(string user, string pass)
        {
            if (string.IsNullOrWhiteSpace(user) || user.Length < 3 || user.Length > 20) return "Username: 3 to 20 characters.";
            foreach (char c in user)
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_' && c != '@') return "Username: letters, numbers and . - _ @ only.";
            if (pass == null || pass.Length < 8 || pass.Length > 30) return "Password: 8 to 30 characters.";
            bool upper = false, lower = false, digit = false, symbol = false;
            foreach (char c in pass)
            {
                if (char.IsUpper(c)) upper = true;
                else if (char.IsLower(c)) lower = true;
                else if (char.IsDigit(c)) digit = true;
                else symbol = true;
            }
            if (!(upper && lower && digit && symbol)) return "Password needs an uppercase, a lowercase, a number and a symbol.";
            return null;
        }

        private static string Friendly(Exception e)
        {
            string m = e.Message.ToLowerInvariant();
            if (m.Contains("exist") || m.Contains("taken") || m.Contains("conflict")) return "That username is already taken.";
            if (m.Contains("invalid username or password") || m.Contains("wrong") || m.Contains("credentials") || m.Contains("not found"))
                return "Wrong username or password.";
            if (m.Contains("password")) return "Password needs 8-30 characters with an uppercase, a lowercase, a number and a symbol.";
            if (m.Contains("network") || m.Contains("timeout")) return "Can't reach the server. Check your connection.";
            return e.Message;
        }

        private static void Fail(Exception e)
        {
            Status = State.Offline;
            Error = e.Message;
            Debug.LogWarning($"Online services unavailable: {e.Message}");
        }
    }
}
