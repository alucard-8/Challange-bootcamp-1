using System;
using UnityEngine;

namespace Soulbreak.Audio
{
    public enum Sfx
    {
        Laser, Bolt, Cannon, Missile, Zap, Lance, Blade, Vortex,
        Explosion, Kill, BigKill, Headshot, ReactorHit, ShieldHit, Nova,
        Orb, Buy, Fail, WaveStart, WaveClear, Streak, Overdrive, Click, Reload, Enrage, Alarm,
        UIHover, UIClick, UIOpen, UIClose, Tally, NewBest, Collapse,
    }

    [CreateAssetMenu(menuName = "Soulbreak/Audio Library", fileName = "AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public class SfxEntry
        {
            public Sfx id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Serializable]
        public class MusicEntry
        {
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        [Header("Sound effects (one slot per Sfx; empty clip = silent)")]
        public SfxEntry[] sfx;

        [Header("Music")]
        [Tooltip("Main-menu / hub loop.")]
        public MusicEntry menuMusic = new MusicEntry();

        [Tooltip("Game music split into intensity tiers (calm → intense). Tiers crossfade as intensity rises.")]
        public MusicEntry[] gameTiers;

        private void Reset() => RebuildSfx();
        private void OnValidate() => RebuildSfx();

        public void RebuildSfx()
        {
            int count = Enum.GetValues(typeof(Sfx)).Length;
            if (sfx == null)
            {
                sfx = new SfxEntry[count];
            }
            else if (sfx.Length != count)
            {
                var old = sfx;
                sfx = new SfxEntry[count];
                Array.Copy(old, sfx, Math.Min(old.Length, count));
            }
            for (int i = 0; i < sfx.Length; i++)
            {
                if (sfx[i] == null) sfx[i] = new SfxEntry();
                sfx[i].id = (Sfx)i;
            }
        }
    }
}