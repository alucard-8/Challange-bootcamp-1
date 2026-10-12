using System;
using System.Collections.Generic;
using UnityEngine;

namespace Soulbreak.Meta
{
    // Persistent between-runs progress (Soul Essence, unlocks, equipped loadout), saved as JSON in PlayerPrefs.
    // Plain service class: no scene objects, so both the menu and the game scene can use it.
    public static class MetaProgress
    {
        [Serializable]
        private class SaveData
        {
            public int essence;
            public List<string> unlocked = new List<string>();
            public string technique = "";
            public string skin = "";
            public int bestWave;
            public string mode = "";
            public List<string> secrets = new List<string>();
            public int difficulty = 1;   // Normal
            public int bestScore;
            public int lastScore;
            public int lastWave;
            public int runs;
            public List<string> seenEnemies = new List<string>();
            public List<string> seenPerks = new List<string>();
            public long savedAt;   // unix ms, so cloud sync can tell which copy is newer
        }

        // ---------- Index (discovered enemies & perks) ----------
        public static bool IsDiscovered(bool enemy, string id) => Seen(enemy).Contains(id);

        // Returns true the first time something is discovered.
        public static bool Discover(bool enemy, string id)
        {
            var list = Seen(enemy);
            if (string.IsNullOrEmpty(id) || list.Contains(id)) return false;
            list.Add(id);
            Save();
            return true;
        }

        public static void DiscoverAll(CodexCatalog codex)
        {
            foreach (var e in codex.enemies) if (e != null && !Seen(true).Contains(e.name)) Seen(true).Add(e.name);
            foreach (var p in codex.perks) if (p != null && !Seen(false).Contains(p.name)) Seen(false).Add(p.name);
            Save();
        }

        private static List<string> Seen(bool enemy)
        {
            if (Data.seenEnemies == null) Data.seenEnemies = new List<string>();
            if (Data.seenPerks == null) Data.seenPerks = new List<string>();
            return enemy ? Data.seenEnemies : Data.seenPerks;
        }

        // ---------- Difficulty & scores ----------
        public static Difficulty SelectedDifficulty
        {
            get => (Difficulty)Mathf.Clamp(Data.difficulty, 0, 3);
            set { Data.difficulty = (int)value; Save(); }
        }

        public static int BestScore => Data.bestScore;
        public static int LastScore => Data.lastScore;
        public static int LastWave => Data.lastWave;
        public static int Runs => Data.runs;

        // Records a finished run. Returns true if it's a new best score.
        public static bool RecordRun(int score, int wave)
        {
            Data.lastScore = score;
            Data.lastWave = wave;
            Data.runs++;
            bool best = score > Data.bestScore;
            if (best) Data.bestScore = score;
            if (wave > Data.bestWave) Data.bestWave = wave;
            Save();
            return best;
        }

        private const string Key = "soulbreak.meta.v1";
        private static SaveData data;

        public static event Action OnChanged;

        private static SaveData Data
        {
            get
            {
                if (data != null) return data;
                string json = PlayerPrefs.GetString(Key, "");
                data = string.IsNullOrEmpty(json) ? new SaveData() : JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                return data;
            }
        }

        public static int Essence => Data.essence;
        public static int BestWave => Data.bestWave;
        public static string EquippedTechnique => Data.technique;
        public static string EquippedSkin => Data.skin;
        public static bool IsUnlocked(MetaItem item) =>
            item.type == MetaItemType.GameMode ? IsSecretUnlocked(item.id) : item.cost == 0 || Data.unlocked.Contains(item.id);

        // ---------- Game modes & secrets ----------
        public static string SelectedMode
        {
            get => Data.mode;
            set { Data.mode = value ?? ""; Save(); }
        }

        public static bool IsSecretUnlocked(string id) => Data.secrets.Contains(id);

        public static void UnlockSecret(string id)
        {
            if (Data.secrets.Contains(id)) return;
            Data.secrets.Add(id);
            Save();
        }

        // Admin: unlock every skin, technique and secret mode in the catalog.
        public static void UnlockAll(MetaCatalog catalog)
        {
            foreach (var i in catalog.items)
            {
                if (i.type == MetaItemType.GameMode) { if (!Data.secrets.Contains(i.id)) Data.secrets.Add(i.id); }
                else if (!Data.unlocked.Contains(i.id)) Data.unlocked.Add(i.id);
            }
            Save();
        }

        public static void AddEssence(int amount)
        {
            Data.essence += Mathf.Max(0, amount);
            Save();
        }

        public static void RecordWave(int wave)
        {
            if (wave <= Data.bestWave) return;
            Data.bestWave = wave;
            Save();
        }

        public static bool TryUnlock(MetaItem item)
        {
            if (IsUnlocked(item)) return true;
            if (Data.essence < item.cost) return false;
            Data.essence -= item.cost;
            Data.unlocked.Add(item.id);
            Save();
            return true;
        }

        // Equip (or unequip if already equipped) an unlocked item.
        public static void ToggleEquip(MetaItem item)
        {
            if (!IsUnlocked(item)) return;
            if (item.type == MetaItemType.StartingTechnique) Data.technique = Data.technique == item.id ? "" : item.id;
            else if (item.type == MetaItemType.GameMode) Data.mode = Data.mode == item.modeId ? "" : item.modeId;
            else Data.skin = item.id;
            Save();
        }

        public static bool IsEquipped(MetaItem item)
        {
            switch (item.type)
            {
                case MetaItemType.StartingTechnique: return Data.technique == item.id;
                case MetaItemType.GameMode: return Data.mode == item.modeId;
                default: return Data.skin == item.id;
            }
        }

        // ---------- Cloud sync (used by CloudProgress) ----------
        public static string ExportJson() => JsonUtility.ToJson(Data);
        public static long SavedAt => Data.savedAt;
        public static bool HasProgress => Data.runs > 0 || Data.essence > 0 || Data.unlocked.Count > 0;

        // Replaces local progress with a copy from the cloud (keeps its timestamp).
        public static void ImportJson(string json)
        {
            var incoming = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SaveData>(json);
            if (incoming == null) return;
            data = incoming;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        private static void Save()
        {
            Data.savedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }
}
