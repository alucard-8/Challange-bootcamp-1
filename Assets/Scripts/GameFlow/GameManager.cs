using System;
using Soulbreak.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Soulbreak.GameFlow
{
    public enum GameState { Playing, GameOver }

    // Owns the game state. Listens for the reactor dying; never polls other systems.
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Reactor.Reactor reactor;

        public GameState State { get; private set; } = GameState.Playing;
        public float SurvivalTime { get; private set; }
        public event Action<GameState> OnStateChanged;

        private void OnEnable() => reactor.OnDestroyed += HandleReactorDestroyed;
        private void OnDisable() => reactor.OnDestroyed -= HandleReactorDestroyed;

        private void Start() => Time.timeScale = 1f;

        private void Update()
        {
            if (State == GameState.Playing)
                SurvivalTime += Time.deltaTime;
            else if (InputReader.RestartPressed())
                Restart();
        }

        private void HandleReactorDestroyed()
        {
            State = GameState.GameOver;
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(State);
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
