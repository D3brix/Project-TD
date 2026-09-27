using System;
using ProjectTD.Enemies;
using ProjectTD.Waves;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectTD.Core
{
    public enum GamePhase
    {
        /// <summary>No wave is running: the player builds, then starts the next wave.</summary>
        Preparation,
        WaveRunning,
        Victory,
        Defeat
    }

    /// <summary>
    /// Connects the waves to the session: kills pay currency, escaped enemies cost lives,
    /// the player starts each wave, and it decides victory or defeat. When the game ends it halts the waves.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] WaveSpawner waveSpawner;
        [SerializeField, Min(1)] int startingLives = 10;
        [SerializeField, Min(0)] int startingCurrency = 50;

        public GameSession Session { get; private set; }
        public WaveSpawner Waves => waveSpawner;

        /// <summary>Raised once, when the game ends in victory or defeat.</summary>
        public event Action<GameOutcome> GameEnded;

        public GamePhase Phase
        {
            get
            {
                switch (Session.Outcome)
                {
                    case GameOutcome.Victory: return GamePhase.Victory;
                    case GameOutcome.Defeat: return GamePhase.Defeat;
                    default: return waveSpawner.IsWaveRunning ? GamePhase.WaveRunning : GamePhase.Preparation;
                }
            }
        }

        public bool CanStartNextWave => !Session.IsOver && waveSpawner.CanStartNextWave;

        void Awake()
        {
            Session = new GameSession(startingLives, startingCurrency);
        }

        void OnEnable()
        {
            waveSpawner.EnemyKilled += HandleEnemyKilled;
            waveSpawner.EnemyReachedEnd += HandleEnemyReachedEnd;
            waveSpawner.AllWavesCompleted += HandleAllWavesCompleted;
        }

        void OnDisable()
        {
            waveSpawner.EnemyKilled -= HandleEnemyKilled;
            waveSpawner.EnemyReachedEnd -= HandleEnemyReachedEnd;
            waveSpawner.AllWavesCompleted -= HandleAllWavesCompleted;
        }

        /// <summary>Starts the next wave, if the game is running and no wave is in progress.</summary>
        public bool StartNextWave()
        {
            return CanStartNextWave && waveSpawner.StartNextWave();
        }

        /// <summary>Reloads the level from scratch: starting lives and currency, no towers, no enemies, wave 0.</summary>
        public void Restart()
        {
            SceneManager.LoadScene(gameObject.scene.buildIndex);
        }

        void HandleEnemyKilled(Enemy enemy)
        {
            Session.AddCurrency(enemy.Reward);
        }

        void HandleEnemyReachedEnd(Enemy enemy)
        {
            if (Session.IsOver)
                return;

            Session.LoseLives(enemy.LivesCost);
            if (Session.Outcome == GameOutcome.Defeat)
                EndGame();
        }

        void HandleAllWavesCompleted()
        {
            if (Session.TryDeclareVictory(waveSpawner.Progress))
                EndGame();
        }

        void EndGame()
        {
            waveSpawner.Halt();
            GameEnded?.Invoke(Session.Outcome);
        }
    }
}
