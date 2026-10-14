using UnityEngine;

namespace Soulbreak.Core
{
    public static class DisplaySettings
    {
        private const string FullscreenKey = "soulbreak.display.fullscreen";
        private const string ResolutionKey = "soulbreak.display.resolution";
        private const string QualityKey = "soulbreak.display.quality";

        public static event System.Action OnChanged;

        private static Resolution[] resolutions;

        public static Resolution[] Resolutions
        {
            get
            {
                if (resolutions == null || resolutions.Length == 0)
                {
                    resolutions = Screen.resolutions;
                    if (resolutions.Length == 0)
                        resolutions = new[] { new Resolution { width = 1920, height = 1080 } };
                }
                return resolutions;
            }
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) > 0;
            set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); Persist(); }
        }

        public static int ResolutionIndex
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(ResolutionKey, DefaultResolutionIndex()), 0, Resolutions.Length - 1);
            set { PlayerPrefs.SetInt(ResolutionKey, Mathf.Clamp(value, 0, Resolutions.Length - 1)); Persist(); }
        }

        public static int QualityLevel
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            set { PlayerPrefs.SetInt(QualityKey, Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1))); Persist(); }
        }

        public static string ResolutionLabel
        {
            get { var r = Resolutions[ResolutionIndex]; return $"{r.width} x {r.height}"; }
        }

        public static string QualityLabel => QualitySettings.names.Length > 0 ? QualitySettings.names[QualityLevel] : "Default";

        private static int DefaultResolutionIndex()
        {
            var res = Resolutions;
            for (int i = 0; i < res.Length; i++)
                if (res[i].width == Screen.width && res[i].height == Screen.height) return i;
            return res.Length - 1;
        }

        private static void Persist()
        {
            Apply();
            OnChanged?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Apply()
        {
            if (PlayerPrefs.HasKey(ResolutionKey))
            {
                var r = Resolutions[ResolutionIndex];
                Screen.SetResolution(r.width, r.height, Fullscreen);
            }
            else
            {
                Screen.fullScreen = Fullscreen;
            }

            if (PlayerPrefs.HasKey(QualityKey)) QualitySettings.SetQualityLevel(QualityLevel, true);
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(FullscreenKey);
            PlayerPrefs.DeleteKey(ResolutionKey);
            PlayerPrefs.DeleteKey(QualityKey);
            Apply();
            OnChanged?.Invoke();
        }
    }
}
