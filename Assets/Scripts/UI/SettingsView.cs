using System;
using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Soulbreak.UI
{
    // Settings panel (used in the main menu and the pause menu): volumes, screen shake, toggles.
    // Values live in GameSettings; this view only displays them and steps them with -/+ buttons.
    public class SettingsView : MonoBehaviour
    {
        public enum Setting { Master, Music, Sfx, ScreenShake, DamageNumbers, Tutorial }

        // One row of the panel. Sliders use minus/plus/bar/value, toggles use the toggle button.
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

        public bool IsOpen { get; private set; }

        private void Awake()
        {
            backButton.onClick.AddListener(Close);
            foreach (var row in rows)
            {
                var r = row;
                if (r.toggle != null) r.toggle.onClick.AddListener(() => SetBool(r.setting, !GetBool(r.setting)));
                if (r.minus != null) r.minus.onClick.AddListener(() => Step(r.setting, -0.1f));
                if (r.plus != null) r.plus.onClick.AddListener(() => Step(r.setting, 0.1f));
            }
        }

        private void OnEnable() => GameSettings.OnChanged += Refresh;
        private void OnDisable() => GameSettings.OnChanged -= Refresh;

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
                    r.value.text = GetBool(r.setting) ? "ON" : "OFF";
                    continue;
                }
                float v = GetFloat(r.setting);
                UIFactory.SetBar(r.bar, v);
                r.value.text = $"{v * 100f:0}%";
            }
        }

        private static void Step(Setting s, float delta) => SetFloat(s, Mathf.Round((GetFloat(s) + delta) * 10f) / 10f);

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

        private static bool GetBool(Setting s) => s == Setting.Tutorial ? GameSettings.Tutorial : GameSettings.DamageNumbers;

        private static void SetBool(Setting s, bool v)
        {
            if (s == Setting.Tutorial) GameSettings.Tutorial = v;
            else GameSettings.DamageNumbers = v;
        }
    }
}
