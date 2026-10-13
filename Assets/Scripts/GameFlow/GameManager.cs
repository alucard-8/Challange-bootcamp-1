using System;

using Soulbreak.Enemies;
using Soulbreak.Perks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Soulbreak.GameFlow
{
    public enum GameState { Combat, Shop, GameOver }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;
        [SerializeField] private WaveManager waves;
        [SerializeField] private PerkShop shop;
        [SerializeField] private Weapons.WeaponRing weapons;
        [SerializeField] private TimeScaleService time;
        [SerializeField] private float firstWaveDelay = 1.5f;

        public GameState State { get; private set; } = GameState.Combat;
        public float SurvivalTime { get; private set; }
        public int WavesCleared { get; private set; }

        public event Action<GameState> OnStateChanged;

        private float collapseTimer = -1f;

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
            time.Clear("gameover");
            Invoke(nameof(BeginFirstWave), firstWaveDelay);
        }

        private void BeginFirstWave() => waves.StartNextWave();

        private void Update()
        {
            if (State == GameState.Combat) SurvivalTime += Time.deltaTime;
            if (State == GameState.GameOver && collapseTimer >= 0f)
            {
                collapseTimer += Time.unscaledDeltaTime;
                if (collapseTimer > 1.4f) { time.Set("gameover", 0f); collapseTimer = -1f; }
            }

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
            weapons.Shuffle();
            waves.StartNextWave();
        }

        private void HandleReactorDestroyed()
        {
            SetState(GameState.GameOver);
            time.Set("gameover", 0.2f);
            collapseTimer = 0f;
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
