using UnityEngine;

namespace Soulbreak.Audio
{
    public static class UIAudio
    {
        private static float lastHover;

        public static void Play(Sfx id, float volume = 0.6f)
        {
            if (id == Sfx.UIHover)
            {
                if (Time.unscaledTime - lastHover < 0.04f) return;
                lastHover = Time.unscaledTime;
            }
            AudioManager.Instance.PlaySfx(id, volume, 0.03f);
        }
    }
}