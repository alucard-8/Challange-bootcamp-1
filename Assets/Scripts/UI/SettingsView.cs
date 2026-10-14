using System;
using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    public class SettingsView : MonoBehaviour
    {
        public enum Setting { Master, Music, Sfx, ScreenShake, DamageNumbers, Tutorial, Colorblind, AimAssist, ReduceFlashing, FpsOverlay, Fullscreen, Resolution, Quality }

        [Serializable]
        private class Row
        {
            public Setting setting;
            public Button minus, plus, toggle;
            public RectTransform bar;
            public Text value;
        }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform panel;
        [SerializeField] private Button backButton;
        [SerializeField] private List<Row> rows = new List<Row>();
        [SerializeField] private UIStrings strings;

        private UIStrings S => strings != null ? strings : UIFactory.Strings;

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(Close);
            foreach (var row in rows)
            {
                var r = row;
                if (r.toggle != null) r.toggle.onClick.AddListener(() => SetBool(r.setting, !GetBool(r.setting)));
                if (r.minus != null) r.minus.onClick.AddListener(() => Adjust(r.setting, -1));
                if (r.plus != null) r.plus.onClick.AddListener(() => Adjust(r.setting, 1));
            }
        }

        private void OnEnable()
        {
            GameSettings.OnChanged += Refresh;
            DisplaySettings.OnChanged += Refresh;
        }

        private void OnDisable()
        {
            GameSettings.OnChanged -= Refresh;
            DisplaySettings.OnChanged -= Refresh;
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Refresh();
            Audio.UIAudio.Play(Audio.Sfx.UIOpen);
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Audio.UIAudio.Play(Audio.Sfx.UIClose);
        }

        public void ResetAll()
        {
            GameSettings.Reset();
            DisplaySettings.Reset();
            Keybinds.Reset();
        }

        private void Update()
        {
            group.alpha = Mathf.MoveTowards(group.alpha, IsOpen ? 1f : 0f, Time.unscaledDeltaTime * 6f);
            group.blocksRaycasts = group.interactable = IsOpen;
            panel.localScale = Vector3.one * UIFactory.PopScale(group.alpha);
        }

        private void Refresh()
        {
            foreach (var r in rows)
            {
                if (r.toggle != null)
                {
                    r.value.text = GetBool(r.setting) ? S.settingsOn : S.settingsOff;
                    continue;
                }
                if (IsOption(r.setting))
                {
                    r.value.text = OptionLabel(r.setting);
                    continue;
                }
                float v = GetFloat(r.setting);
                if (r.bar != null) UIFactory.SetBar(r.bar, v);
                r.value.text = string.Format(S.settingsPercent, v * 100f);
            }
        }

        private static void Adjust(Setting s, int dir)
        {
            if (IsOption(s)) { CycleOption(s, dir); return; }
            if (IsBool(s)) { SetBool(s, !GetBool(s)); return; }
            SetFloat(s, Mathf.Round((GetFloat(s) + 0.1f * dir) * 10f) / 10f);
        }

        private static bool IsOption(Setting s) => s == Setting.Resolution || s == Setting.Quality;

        private static string OptionLabel(Setting s) =>
            s == Setting.Resolution ? DisplaySettings.ResolutionLabel : DisplaySettings.QualityLabel;

        private static void CycleOption(Setting s, int dir)
        {
            if (s == Setting.Resolution)
            {
                int n = DisplaySettings.Resolutions.Length;
                DisplaySettings.ResolutionIndex = (DisplaySettings.ResolutionIndex + dir + n) % n;
            }
            else
            {
                int n = Mathf.Max(1, QualitySettings.names.Length);
                DisplaySettings.QualityLevel = (DisplaySettings.QualityLevel + dir + n) % n;
            }
        }

        private static float GetFloat(Setting s)
        {
            switch (s)
            {
                case Setting.Master: return GameSettings.Master;
                case Setting.Music: return GameSettings.Music;
                case Setting.Sfx: return GameSettings.Sfx;
                default: return GameSettings.ScreenShake;
            }
        }

        private static void SetFloat(Setting s, float v)
        {
            switch (s)
            {
                case Setting.Master: GameSettings.Master = v; break;
                case Setting.Music: GameSettings.Music = v; break;
                case Setting.Sfx: GameSettings.Sfx = v; break;
                default: GameSettings.ScreenShake = v; break;
            }
        }

        private static bool IsBool(Setting s) => s != Setting.Master && s != Setting.Music && s != Setting.Sfx && s != Setting.ScreenShake;

        private static bool GetBool(Setting s)
        {
            switch (s)
            {
                case Setting.Tutorial: return GameSettings.Tutorial;
                case Setting.DamageNumbers: return GameSettings.DamageNumbers;
                case Setting.Colorblind: return GameSettings.Colorblind;
                case Setting.AimAssist: return GameSettings.AimAssist;
                case Setting.ReduceFlashing: return GameSettings.ReduceFlashing;
                case Setting.FpsOverlay: return GameSettings.FpsOverlay;
                case Setting.Fullscreen: return DisplaySettings.Fullscreen;
                default: return false;
            }
        }

        private static void SetBool(Setting s, bool v)
        {
            switch (s)
            {
                case Setting.Tutorial: GameSettings.Tutorial = v; break;
                case Setting.DamageNumbers: GameSettings.DamageNumbers = v; break;
                case Setting.Colorblind: GameSettings.Colorblind = v; break;
                case Setting.AimAssist: GameSettings.AimAssist = v; break;
                case Setting.ReduceFlashing: GameSettings.ReduceFlashing = v; break;
                case Setting.FpsOverlay: GameSettings.FpsOverlay = v; break;
                case Setting.Fullscreen: DisplaySettings.Fullscreen = v; break;
            }
        }
    }
}
