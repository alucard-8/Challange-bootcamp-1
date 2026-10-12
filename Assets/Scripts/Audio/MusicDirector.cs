using Soulbreak.Enemies;
using Soulbreak.GameFlow;
using Soulbreak.Player;
using UnityEngine;

namespace Soulbreak.Audio
{
    // Reads game state and steers the music: intensity grows with the wave number and with danger
    // near the reactor, maxes out in Overdrive / boss fights; muffled while paused or shopping.
    public class MusicDirector : MonoBehaviour
    {
        [SerializeField] private MusicSynth music;
        [SerializeField] private GameManager game;
        [SerializeField] private WaveManager waves;
        [SerializeField] private EnemyRegistry registry;
        [SerializeField] private PauseController pause;
        [SerializeField] private OverdriveSystem overdrive;
        [SerializeField] private Transform reactor;

        private void Update()
        {
            float intensity = 0.2f + Mathf.Min(0.45f, waves.Wave * 0.06f);

            float closest = float.MaxValue;
            bool boss = false;
            foreach (var e in registry.Alive)
            {
                if (e == null) continue;
                closest = Mathf.Min(closest, Vector2.Distance(e.transform.position, reactor.position));
                boss |= e.Config.isBoss;
            }
            intensity += Mathf.Clamp01((7f - closest) / 5f) * 0.3f;
            if (boss) intensity = Mathf.Max(intensity, 0.85f);
            if (overdrive.IsActive) intensity = 1f;
            if (game.State == GameState.Shop) intensity = 0.25f;
            if (game.State == GameState.GameOver) intensity = 0.05f;

            music.Intensity = intensity;
            music.Muffle = Mathf.Max(pause.Blend, game.State == GameState.Shop ? 0.55f : 0f, game.State == GameState.GameOver ? 0.8f : 0f);
        }
    }
}
