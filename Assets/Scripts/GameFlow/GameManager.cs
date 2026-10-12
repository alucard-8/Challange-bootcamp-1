using System;

using Soulbreak.Enemies;
using Soulbreak.Perks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Soulbreak.GameFlow
{
    public enum GameState { Combat, Shop, GameOver }

    // Mediator for the game loop: wave cleared -> open shop -> shop closed -> next wave.
    // WaveManager and PerkShop never reference each other; only this class connects them.
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private WaveManager waves;
        [SerializeField] private PerkShop shop;
        [SerializeField] private Weapons.WeaponRing weapons;
        [SerializeField] private float firstWaveDelay = 1.5f;

        public GameState State { get; private set; } = GameState.Combat;
        public float SurvivalTime { get; private set; }
        public int WavesCleared { get; private set; }

        public event Action<GameState> OnStateChanged;

        private void OnEnable()
        {
            reactor.OnDestroyed += HandleReactorDestroyed;
            waves.OnWaveCleared += HandleWaveCleared;
            shop.OnClosed += HandleShopClosed;
        }

        private void OnDisable()
        {
            reactor.OnDestroyed -= HandleReactorDestroyed;
            waves.OnWaveCleared -= HandleWaveCleared;
            shop.OnClosed -= HandleShopClosed;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            Invoke(nameof(BeginFirstWave), firstWaveDelay);
        }

        private void BeginFirstWave() => waves.StartNextWave();

        private void Update()
        {
            if (State == GameState.Combat) SurvivalTime += Time.deltaTime;

        }

        private void HandleWaveCleared(int wave)
        {
            if (State == GameState.GameOver) return;
            WavesCleared = wave;
            SetState(GameState.Shop);
            shop.Open();
        }

        private void HandleShopClosed()
        {
            if (State == GameState.GameOver) return;
            SetState(GameState.Combat);
            weapons.Shuffle();   // new random module layout every wave
            waves.StartNextWave();
        }

        private void HandleReactorDestroyed()
        {
            SetState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        private void SetState(GameState state)
        {
            State = state;
            OnStateChanged?.Invoke(state);
        }

        public void Restart() => SceneFader.LoadScene(SceneManager.GetActiveScene().name, 0.4f);

        public void ToMainMenu() => SceneFader.LoadScene(SceneNames.MainMenu, 0.5f);

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
