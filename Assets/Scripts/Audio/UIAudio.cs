using UnityEngine;

namespace Soulbreak.Audio
{
    // Interface sounds (hover, click, panel open/close, tally ticks). One persistent AudioSource,
    // works in every scene and while the game is paused. Volume follows the SFX setting.
    public static class UIAudio
    {
        private static AudioSource source;
        private static float lastHover;

        private static AudioSource Source
        {
            get
            {
                if (source != null) return source;
                var go = new GameObject("UIAudio");
                Object.DontDestroyOnLoad(go);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = true;
                return source;
            }
        }

        public static void Play(Sfx id, float volume = 0.6f)
        {
            if (id == Sfx.UIHover)
            {
                if (Time.unscaledTime - lastHover < 0.04f) return;
                lastHover = Time.unscaledTime;
            }
            Source.pitch = 1f + Random.Range(-0.03f, 0.03f);
            Source.PlayOneShot(SfxSynth.Get(id), volume * Core.GameSettings.Sfx);
        }
    }
}
