using System;
using System.Collections.Generic;
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

    // Generates every sound effect procedurally (no audio files needed): oscillators + noise +
    // pitch sweeps + envelopes written straight into AudioClips, cached on first use.
    public static class SfxSynth
    {
        private const int Rate = 44100;
        private const float Tau = Mathf.PI * 2f;
        private static readonly Dictionary<Sfx, AudioClip> Cache = new Dictionary<Sfx, AudioClip>();
        private static readonly System.Random Rng = new System.Random(1234);
        private static float lp;

        public static AudioClip Get(Sfx id)
        {
            if (Cache.TryGetValue(id, out var clip) && clip != null) return clip;
            clip = Build(id);
            Cache[id] = clip;
            return clip;
        }

        private static AudioClip Build(Sfx id)
        {
            switch (id)
            {
                case Sfx.Laser: return Make(id, 0.14f, t => Sq(Sw(t, 1400, 320, 0.14f)) * 0.3f * Env(t, 0.002f, 0.14f) + N() * 0.08f * Env(t, 0, 0.03f));
                case Sfx.Bolt: return Make(id, 0.09f, t => Saw(Sw(t, 1800, 900, 0.09f)) * 0.28f * Env(t, 0.001f, 0.09f));
                case Sfx.Cannon: return Make(id, 0.55f, t => (Sin(Sw(t, 180, 40, 0.5f)) * 0.8f + N() * 0.35f * Env(t, 0, 0.15f)) * Env(t, 0.003f, 0.55f));
                case Sfx.Missile: return Make(id, 0.32f, t => (N() * 0.22f + Saw(Sw(t, 300, 900, 0.32f)) * 0.2f) * Env(t, 0.02f, 0.32f));
                case Sfx.Zap: return Make(id, 0.3f, t => (Sq(t * 900 + Mathf.Sin(t * 180) * 4f) * 0.22f + N() * 0.15f) * Env(t, 0.001f, 0.3f));
                case Sfx.Lance: return Make(id, 0.6f, t => (Saw(t * 80) * 0.35f + Saw(Sw(t, 2400, 600, 0.6f)) * 0.22f + N() * 0.12f) * Env(t, 0.005f, 0.6f));
                case Sfx.Blade: return Make(id, 0.5f, t => (Sin(t * 900) + Sin(t * 1340) * 0.7f + Sin(t * 2100) * 0.5f) * 0.22f * Env(t, 0.001f, 0.5f));
                case Sfx.Vortex: return Make(id, 1.4f, t => (Sin(Sw(t, 120, 30, 1.4f)) * 0.6f + N() * 0.2f * (0.5f + 0.5f * Mathf.Sin(t * 30))) * Env(t, 0.1f, 1.4f));
                case Sfx.Explosion: return Make(id, 0.7f, t => (Lp(0.15f) * 0.9f + Sin(Sw(t, 90, 30, 0.7f)) * 0.5f) * Env(t, 0.002f, 0.7f));
                case Sfx.Kill: return Make(id, 0.16f, t => (N() * 0.35f + Sq(Sw(t, 400, 120, 0.16f)) * 0.22f) * Env(t, 0.001f, 0.16f));
                case Sfx.BigKill: return Make(id, 0.45f, t => (Lp(0.25f) * 0.8f + Sq(Sw(t, 220, 50, 0.45f)) * 0.28f) * Env(t, 0.002f, 0.45f));
                case Sfx.Headshot: return Make(id, 0.4f, t => (Sin(t * 1760) * 0.45f + Sin(t * 2640) * 0.3f + Sin(t * 3520) * 0.15f) * Env(t, 0.001f, 0.4f));
                case Sfx.ReactorHit: return Make(id, 0.5f, t => (Sin(Sw(t, 90, 40, 0.5f)) * 0.9f + Lp(0.3f) * 0.5f) * Env(t, 0.002f, 0.5f));
                case Sfx.ShieldHit: return Make(id, 0.4f, t => (Sin(t * 1200) * 0.4f + Sin(t * 1810) * 0.3f + N() * 0.1f) * Env(t, 0.001f, 0.4f));
                case Sfx.Nova: return Make(id, 0.9f, t => (Lp(0.1f) * 0.6f + Sin(Sw(t, 60, 200, 0.9f)) * 0.5f) * Env(t, 0.01f, 0.9f));
                case Sfx.Orb: return Make(id, 0.3f, t => Sin(t * Arp(t, 0.06f, 880, 1108, 1318, 1760)) * 0.28f * Env(t, 0.001f, 0.3f));
                case Sfx.Buy: return Make(id, 0.35f, t => (Sq(t * Arp(t, 0.07f, 988, 1318)) * 0.18f + Sin(t * Arp(t, 0.07f, 1976, 2637)) * 0.15f) * Env(t, 0.001f, 0.35f));
                case Sfx.Fail: return Make(id, 0.25f, t => Sq(t * Arp(t, 0.1f, 220, 160)) * 0.22f * Env(t, 0.001f, 0.25f));
                case Sfx.WaveStart: return Make(id, 1.0f, t => (Saw(t * 110) + Saw(t * 110.8f) + Saw(t * 165) + Saw(t * 220.5f)) * 0.1f * Env(t, 0.15f, 1.0f));
                case Sfx.WaveClear: return Make(id, 0.8f, t => (Sin(t * Arp(t, 0.1f, 523, 659, 784, 1047)) * 0.3f + Sin(t * Arp(t, 0.1f, 1047, 1318, 1568, 2093)) * 0.12f) * Env(t, 0.005f, 0.8f));
                case Sfx.Streak: return Make(id, 0.6f, t => (Saw(t * Arp(t, 0.05f, 440, 554, 659, 880, 1108, 1318)) * 0.16f + N() * 0.04f) * Env(t, 0.002f, 0.6f));
                case Sfx.Overdrive: return Make(id, 1.2f, t => (Saw(Sw(t, 80, 640, 1.2f)) * 0.22f + Saw(Sw(t, 81, 650, 1.2f)) * 0.22f + Lp(0.4f) * 0.2f) * Env(t, 0.05f, 1.2f));
                case Sfx.Click: return Make(id, 0.04f, t => Sq(t * 2200) * 0.12f * Env(t, 0, 0.04f));
                case Sfx.Reload: return Make(id, 0.22f, t => (N() * 0.2f * Env(t, 0, 0.04f) + Sq(t * (t < 0.12f ? 600 : 900)) * 0.1f) * Env(t, 0.001f, 0.22f));
                case Sfx.Enrage: return Make(id, 1.3f, t => (Saw(t * 55) * 0.35f + Saw(t * 58) * 0.35f + Lp(0.2f) * 0.35f) * Env(t, 0.05f, 1.3f) * (0.7f + 0.3f * Mathf.Sin(t * 40)));
                case Sfx.Alarm: return Make(id, 0.5f, t => Sq(t * Arp(t, 0.25f, 880, 660)) * 0.13f * Env(t, 0.005f, 0.5f));
                case Sfx.UIHover: return Make(id, 0.05f, t => Sin(t * 1900) * 0.12f * Env(t, 0.002f, 0.05f));
                case Sfx.UIClick: return Make(id, 0.09f, t => (Sin(Sw(t, 1400, 700, 0.09f)) * 0.25f + N() * 0.05f * Env(t, 0, 0.01f)) * Env(t, 0.001f, 0.09f));
                case Sfx.UIOpen: return Make(id, 0.28f, t => (Lp(0.08f) * 0.35f + Sin(Sw(t, 300, 900, 0.28f)) * 0.12f) * Env(t, 0.06f, 0.28f));
                case Sfx.UIClose: return Make(id, 0.22f, t => (Lp(0.08f) * 0.3f + Sin(Sw(t, 800, 280, 0.22f)) * 0.1f) * Env(t, 0.02f, 0.22f));
                case Sfx.Tally: return Make(id, 0.06f, t => Sq(t * 1500) * 0.08f * Env(t, 0.001f, 0.06f));
                case Sfx.NewBest: return Make(id, 1.0f, t => (Saw(t * Arp(t, 0.09f, 523, 659, 784, 1047, 1319)) * 0.14f + Sin(t * Arp(t, 0.09f, 1047, 1319, 1568, 2093, 2637)) * 0.12f) * Env(t, 0.003f, 1.0f));
                case Sfx.Collapse: return Make(id, 2.0f, t => (Lp(0.05f) * 0.7f + Sin(Sw(t, 90, 25, 2f)) * 0.6f + Saw(Sw(t, 300, 40, 2f)) * 0.08f) * Env(t, 0.01f, 2.0f));
            }
            return Make(id, 0.05f, t => 0f);
        }

        private static AudioClip Make(Sfx id, float seconds, Func<float, float> wave)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            lp = 0f;
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(wave(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create("sfx_" + id, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // ---------- tiny synth toolkit (all take "cycles" = frequency integrated over time) ----------
        private static float Sin(float cycles) => Mathf.Sin(cycles * Tau);
        private static float Sq(float cycles) => (cycles - Mathf.Floor(cycles)) < 0.5f ? 1f : -1f;
        private static float Saw(float cycles) => (cycles - Mathf.Floor(cycles)) * 2f - 1f;
        private static float N() => (float)(Rng.NextDouble() * 2.0 - 1.0);
        private static float Lp(float k) { lp += (N() - lp) * k; return lp * 2.5f; }

        // Cycles for a linear pitch sweep from f0 to f1 over dur seconds (then holding f1).
        private static float Sw(float t, float f0, float f1, float dur)
        {
            float tt = Mathf.Min(t, dur);
            float c = f0 * tt + (f1 - f0) * tt * tt / (2f * dur);
            if (t > dur) c += f1 * (t - dur);
            return c;
        }

        private static float Arp(float t, float stepTime, params float[] notes) => notes[Mathf.Min(notes.Length - 1, (int)(t / stepTime))];

        // Attack then quadratic decay to zero at "length".
        private static float Env(float t, float attack, float length)
        {
            if (t < attack) return attack <= 0f ? 1f : t / attack;
            float k = Mathf.Clamp01((t - attack) / Mathf.Max(0.0001f, length - attack));
            return (1f - k) * (1f - k);
        }
    }
}
