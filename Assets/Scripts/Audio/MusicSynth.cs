using UnityEngine;

namespace Soulbreak.Audio
{
    // Procedural synthwave soundtrack rendered on the audio thread (streaming AudioClip).
    // A minor progression (Am - F - C - G) with layers that fade in with Intensity:
    //   0.00 pad + bass   0.25 kick   0.50 hats + arpeggio   0.75 snare   0.90 lead
    // Muffle applies a low-pass (used while paused / shopping).
    public class MusicSynth : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float volume = 0.3f;
        [SerializeField] private float bpm = 112f;
        [SerializeField, Range(0f, 1f)] private float startIntensity = 0.15f;

        private const int Rate = 44100;
        // Variant 0 (Soul Reactor): Am - F - C - G, square arp.
        // Variant 1 (Star Core):   Dm - Bb - F - C (spacey), soft sine arp with shimmer.
        private static readonly int[][][] Progressions =
        {
            new[] { new[] { 57, 60, 64 }, new[] { 53, 57, 60 }, new[] { 48, 52, 55 }, new[] { 55, 59, 62 } },
            new[] { new[] { 50, 53, 57 }, new[] { 46, 50, 53 }, new[] { 53, 57, 60 }, new[] { 48, 52, 55 } },
        };

        private volatile int variant;
        public void SetVariant(int v) => variant = Mathf.Clamp(v, 0, Progressions.Length - 1);

        // Written from the main thread, read on the audio thread.
        private volatile float targetIntensity;
        private volatile float targetMuffle;

        public float Intensity { set => targetIntensity = Mathf.Clamp01(value); }
        public float Muffle { set => targetMuffle = Mathf.Clamp01(value); }

        // Audio-thread state
        private long sample;
        private float intensity, muffle;
        private float padL, padR, bassPhase, arpPhase, leadPhase, kickPhase;
        private readonly float[] padPhases = new float[6];
        private float bassLp, padLpL, padLpR, hatLp, outLpL, outLpR;
        private uint noiseSeed = 22222;

        private void Start()
        {
            targetIntensity = intensity = startIntensity;
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = AudioClip.Create("SoulbreakMusic", Rate * 4, 2, Rate, true, OnAudioRead);
            src.loop = true;
            src.spatialBlend = 0f;
            src.volume = volume;
            src.ignoreListenerPause = true;
            src.Play();
            source = src;
        }

        private AudioSource source;

        // Follow the music volume setting live.
        private void Update()
        {
            if (source != null) source.volume = volume * Core.GameSettings.Music;
        }

        private static float MidiToHz(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

        private float Noise()
        {
            noiseSeed ^= noiseSeed << 13; noiseSeed ^= noiseSeed >> 17; noiseSeed ^= noiseSeed << 5;
            return (noiseSeed / (float)uint.MaxValue) * 2f - 1f;
        }

        private static float Layer(float intensity, float from) => Mathf.Clamp01((intensity - from) / 0.15f);

        private void OnAudioRead(float[] data)
        {
            float secondsPerStep = 60f / bpm / 4f;   // 16th notes
            for (int i = 0; i < data.Length; i += 2)
            {
                intensity += (targetIntensity - intensity) * 0.00002f;
                muffle += (targetMuffle - muffle) * 0.0001f;

                double t = sample / (double)Rate;
                long step = (long)(t / secondsPerStep);
                float stepT = (float)(t - step * secondsPerStep);   // time since this 16th started
                int bar = (int)(step / 16 % 4);
                int s16 = (int)(step % 16);
                int[] chord = Progressions[variant][bar];
                bool star = variant == 1;

                // Pad: detuned saws on the chord, softly low-passed
                float pl = 0f, pr = 0f;
                for (int v = 0; v < 3; v++)
                {
                    float f = MidiToHz(chord[v] + 12);
                    padPhases[v] = Frac(padPhases[v] + f * 1.003f / Rate);
                    padPhases[v + 3] = Frac(padPhases[v + 3] + f * 0.997f / Rate);
                    pl += padPhases[v] * 2f - 1f;
                    pr += padPhases[v + 3] * 2f - 1f;
                }
                padLpL += (pl - padLpL) * 0.03f;
                padLpR += (pr - padLpR) * 0.03f;
                float pad = 0.05f;

                // Bass: root on 8th notes with a plucky filter envelope
                float bassF = MidiToHz(chord[0] - 12);
                bassPhase = Frac(bassPhase + bassF / Rate);
                float bassEnv = Mathf.Exp(-stepT * (s16 % 2 == 0 ? 9f : 14f));
                bassLp += ((bassPhase * 2f - 1f) - bassLp) * (0.02f + 0.25f * bassEnv);
                float bass = bassLp * 0.22f * (0.6f + 0.4f * bassEnv);

                // Kick on every beat
                float kick = 0f;
                if (s16 % 4 == 0)
                {
                    float kt = stepT;
                    float kf = 45f + 90f * Mathf.Exp(-kt * 30f);
                    kickPhase = Frac(kickPhase + kf / Rate);
                    kick = Mathf.Sin(kickPhase * Mathf.PI * 2f) * Mathf.Exp(-kt * 9f) * 0.5f * Layer(intensity, 0.25f);
                }
                else kickPhase = 0f;

                // Hats on off-beat 8ths
                float n = Noise();
                hatLp += (n - hatLp) * 0.5f;
                float hat = (s16 % 4 == 2) ? (n - hatLp) * Mathf.Exp(-stepT * 60f) * 0.12f * Layer(intensity, 0.5f) : 0f;

                // Snare on beats 2 and 4
                float snare = (s16 == 4 || s16 == 12) ? (n * 0.5f + Mathf.Sin((float)t * 200f * Mathf.PI * 2f) * 0.3f) * Mathf.Exp(-stepT * 18f) * 0.22f * Layer(intensity, 0.75f) : 0f;

                // Arpeggio: chord tones cycling on 16ths
                int arpNote = chord[s16 % 3] + 24 + (s16 % 6 >= 3 ? 12 : 0);
                arpPhase = Frac(arpPhase + MidiToHz(arpNote) / Rate);
                float arpWave = star ? Mathf.Sin(arpPhase * Mathf.PI * 2f) * 1.6f : (arpPhase < 0.5f ? 1f : -1f);
                float arp = arpWave * Mathf.Exp(-stepT * (star ? 9f : 22f)) * 0.05f * Layer(intensity, star ? 0.2f : 0.5f);

                // Lead: saw an octave above the arp, only at high intensity
                leadPhase = Frac(leadPhase + MidiToHz(arpNote + 12) / Rate);
                float lead = (leadPhase * 2f - 1f) * Mathf.Exp(-stepT * 14f) * 0.03f * Layer(intensity, 0.9f);

                float mono = bass + kick + hat + snare + arp + lead;
                float l = mono + padLpL * pad;
                float r = mono + padLpR * pad;

                // Muffle (low-pass) for pause / shop
                float k = Mathf.Lerp(1f, 0.04f, muffle);
                outLpL += (l - outLpL) * k;
                outLpR += (r - outLpR) * k;

                data[i] = Mathf.Clamp(outLpL, -1f, 1f);
                data[i + 1] = Mathf.Clamp(outLpR, -1f, 1f);
                sample++;
            }
        }

        private static float Frac(float x) => x - Mathf.Floor(x);
    }
}
