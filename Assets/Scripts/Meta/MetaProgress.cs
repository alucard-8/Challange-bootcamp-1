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
        public static bool IsUnlocked(MetaItem item) => item.cost == 0 || Data.unlocked.Contains(item.id);

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
            else Data.skin = item.id;
            Save();
        }

        public static bool IsEquipped(MetaItem item) =>
            item.type == MetaItemType.StartingTechnique ? Data.technique == item.id : Data.skin == item.id;

        private static void Save()
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }
    }
}
