using System;
using UnityEngine;

namespace Soulbreak.Core
{
    public static class GameSettings
    {
        private const string Prefix = "soulbreak.settings.";

        public static event Action OnChanged;

        public static float Master { get => Get("master", 0.8f); set => Set("master", value); }
        public static float Music { get => Get("music", 0.7f); set => Set("music", value); }
        public static float Sfx { get => Get("sfx", 0.8f); set => Set("sfx", value); }
        public static float ScreenShake { get => Get("shake", 1f); set => Set("shake", value); }
        public static bool Tutorial { get => Get("tutorial", 1f) > 0.5f; set => Set("tutorial", value ? 1f : 0f); }
        public static bool DamageNumbers { get => Get("dmgnum", 1f) > 0.5f; set => Set("dmgnum", value ? 1f : 0f); }

        public static bool Colorblind { get => Get("colorblind", 0f) > 0.5f; set => Set("colorblind", value ? 1f : 0f); }

        public static bool AimAssist { get => Get("aimassist", 0f) > 0.5f; set => Set("aimassist", value ? 1f : 0f); }

        public static bool ReduceFlashing { get => Get("noflash", 0f) > 0.5f; set => Set("noflash", value ? 1f : 0f); }

        public static bool FpsOverlay { get => Get("fps", 0f) > 0.5f; set => Set("fps", value ? 1f : 0f); }

        public static bool PauseOnUnfocus { get => Get("pauseunfocus", 1f) > 0.5f; set => Set("pauseunfocus", value ? 1f : 0f); }

        public static int Difficulty { get => Mathf.RoundToInt(GetRaw("difficulty", 1f)); set => SetRaw("difficulty", Mathf.Clamp(value, 0, 2)); }

        public static int Seed { get => Mathf.RoundToInt(GetRaw("seed", 0f)); set => SetRaw("seed", Mathf.Max(0, value)); }

        private static float Get(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);
        private static float GetRaw(string key, float fallback) => PlayerPrefs.GetFloat(Prefix + key, fallback);

        private static void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(Prefix + key, Mathf.Clamp01(value));
            Persist();
        }

        private static void SetRaw(string key, float value)
        {
            PlayerPrefs.SetFloat(Prefix + key, value);
            Persist();
        }

        private static void Persist()
        {
            PlayerPrefs.Save();
            Apply();
            OnChanged?.Invoke();
        }

        public static void Reset()
        {
            Master = 0.8f; Music = 0.7f; Sfx = 0.8f; ScreenShake = 1f;
            Tutorial = true; DamageNumbers = true;
            Colorblind = false; AimAssist = false; ReduceFlashing = false; FpsOverlay = false;
            PauseOnUnfocus = true;
            SetRaw("difficulty", 1f);
            SetRaw("seed", 0f);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Apply() => AudioListener.volume = Master;
    }
}
