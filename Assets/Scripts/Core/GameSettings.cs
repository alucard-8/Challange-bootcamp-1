using System;
using UnityEngine;

namespace Soulbreak.Core
{
    // Player options, persisted in PlayerPrefs. Systems read these values; the settings menu writes them.
    public static class GameSettings
    {
        private const string Prefix = "soulbreak.settings.";

        public static event Action OnChanged;

        public static float Master { get => Get("master", 0.8f); set => Set("master", value); }
        public static float Music { get => Get("music", 0.7f); set => Set("music", value); }
        public static float Sfx { get => Get("sfx", 0.8f); set => Set("sfx", value); }
        public static float ScreenShake { get => Get("shake", 1f); set => Set("shake", value); }
        // On until the tutorial is finished or skipped once; the settings menu can switch it back on.
        public static bool Tutorial { get => Get("tutorial", 1f) > 0.5f; set => Set("tutorial", value ? 1f : 0f); }
        public static bool DamageNumbers { get => Get("dmgnum", 1f) > 0.5f; set => Set("dmgnum", value ? 1f : 0f); }

        private static float Get(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);

        private static void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(Prefix + key, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Apply();
            OnChanged?.Invoke();
        }

        // Master volume goes straight to the listener; the rest is read by audio/camera systems.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Apply() => AudioListener.volume = Master;
    }
}
