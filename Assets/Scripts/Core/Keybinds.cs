using System;
using UnityEngine;

namespace Soulbreak.Core
{
    public static class Keybinds
    {
        public enum Action { Nova, Overdrive, Pause, Restart, SkipTutorial, Left, Right }

        private const string Key = "soulbreak.keybind.";

        public static event System.Action OnChanged;

        private static readonly (Action action, string name)[] Defaults =
        {
            (Action.Nova, "Space"),
            (Action.Overdrive, "Q"),
            (Action.Pause, "Escape"),
            (Action.Restart, "R"),
            (Action.SkipTutorial, "T"),
            (Action.Left, "A"),
            (Action.Right, "D"),
        };

        public static string GetName(Action a)
        {
            string stored = PlayerPrefs.GetString(Key + a, "");
            if (!string.IsNullOrEmpty(stored)) return stored;
            foreach (var d in Defaults) if (d.action == a) return d.name;
            return "";
        }

        public static void Set(Action a, string keyName)
        {
            if (string.IsNullOrEmpty(keyName)) return;
            PlayerPrefs.SetString(Key + a, keyName);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        public static void Reset()
        {
            foreach (var d in Defaults) PlayerPrefs.SetString(Key + d.action, d.name);
            PlayerPrefs.Save();
            OnChanged?.Invoke();
        }

        public static bool IsDefault(Action a)
        {
            foreach (var d in Defaults) if (d.action == a) return GetName(a) == d.name;
            return true;
        }
    }
}
