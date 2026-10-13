using System.Collections.Generic;
using Soulbreak.Core;
using UnityEngine;

namespace Soulbreak.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const int PoolSize = 16;
        private const float SpamGuard = 0.035f;

        private static AudioManager instance;
        public static AudioManager Instance => instance != null ? instance : Create();

        [SerializeField] private AudioLibrary library;

        private AudioLibrary lib;
        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
        private int next;

        private AudioSource menuSource;
        private readonly List<AudioSource> tiers = new List<AudioSource>();
        private int musicState;
        private float intensity, muffle, shownMuffle;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < PoolSize; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                s.ignoreListenerPause = true;
                sfxPool.Add(s);
            }
        }

        private static AudioManager Create()
        {
            var go = new GameObject("AudioManager");
            return go.AddComponent<AudioManager>();
        }

        public AudioLibrary Library
        {
            get
            {
                if (lib == null) lib = library != null ? library : Resources.Load<AudioLibrary>("AudioLibrary");
                return lib;
            }
        }

        public void PlaySfx(Sfx id, float volume = 1f, float pitchVariance = 0.08f)
        {
            if (lastPlayed.TryGetValue(id, out float last) && Time.unscaledTime - last < SpamGuard) return;
            lastPlayed[id] = Time.unscaledTime;

            var entry = EntryFor(id);
            if (entry == null || entry.clip == null) return;

            var src = sfxPool[next];
            next = (next + 1) % sfxPool.Count;
            src.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            src.PlayOneShot(entry.clip, entry.volume * volume * GameSettings.Sfx);
        }

        private AudioLibrary.SfxEntry EntryFor(Sfx id)
        {
            var a = Library;
            if (a == null || a.sfx == null) return null;
            int i = (int)id;
            return i >= 0 && i < a.sfx.Length ? a.sfx[i] : null;
        }

        public void SetIntensity(float value) => intensity = Mathf.Clamp01(value);
        public void SetMuffle(float value) => muffle = Mathf.Clamp01(value);

        public void PlayMenuMusic() { if (musicState == 1) return; StopMusic(); StartMenuMusic(); }
        public void PlayGameMusic() { if (musicState == 2) return; StopMusic(); StartGameMusic(); }

        public void StopMusic()
        {
            musicState = 0;
            if (menuSource != null) menuSource.Stop();
            foreach (var s in tiers) s.Stop();
        }

        private void StartMenuMusic()
        {
            var entry = Library != null ? Library.menuMusic : null;
            if (entry == null || entry.clip == null) return;
            if (menuSource == null) { menuSource = MakeMusicSource(); }
            menuSource.clip = entry.clip;
            menuSource.volume = entry.volume * GameSettings.Music;
            menuSource.Play();
            musicState = 1;
        }

        private void StartGameMusic()
        {
            var a = Library;
            if (a == null || a.gameTiers == null || a.gameTiers.Length == 0) return;
            while (tiers.Count < a.gameTiers.Length) tiers.Add(MakeMusicSource());
            for (int i = 0; i < a.gameTiers.Length; i++)
            {
                var e = a.gameTiers[i];
                if (e == null || e.clip == null) continue;
                tiers[i].clip = e.clip;
                tiers[i].Play();
            }
            musicState = 2;
        }

        private AudioSource MakeMusicSource()
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = true;
            s.spatialBlend = 0f;
            s.ignoreListenerPause = true;
            return s;
        }

        private void Update()
        {
            shownMuffle = Mathf.MoveTowards(shownMuffle, muffle, Time.unscaledDeltaTime * 2f);
            var a = Library;

            if (musicState == 1 && menuSource != null && a != null && a.menuMusic != null)
            {
                menuSource.volume = a.menuMusic.volume * GameSettings.Music * Dip();
            }
            else if (musicState == 2 && a != null && a.gameTiers != null && tiers.Count > 0)
            {
                int n = a.gameTiers.Length;
                float level = Mathf.Clamp(intensity, TierLevel(0, n), 1f);
                for (int i = 0; i < n; i++)
                {
                    var e = a.gameTiers[i];
                    if (e == null || e.clip == null || tiers[i].clip == null) continue;
                    float lo = i > 0 ? TierLevel(i - 1, n) : -1f;
                    float hi = i < n - 1 ? TierLevel(i + 1, n) : 2f;
                    float c = TierLevel(i, n);
                    float w = level <= c ? Mathf.InverseLerp(lo, c, level) : 1f - Mathf.InverseLerp(c, hi, level);
                    tiers[i].volume = e.volume * GameSettings.Music * Dip() * w;
                }
            }
        }

        private static float TierLevel(int i, int count) => count <= 1 ? 0f : (float)i / (count - 1);

        private float Dip() => Mathf.Lerp(1f, 0.4f, shownMuffle);
    }
}