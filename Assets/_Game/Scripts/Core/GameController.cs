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
    /// the player starts each wave (or lets Auto Wave start it after a short countdown), and it decides victory or defeat.
    /// When the game ends it halts the waves.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] WaveSpawner waveSpawner;
        [SerializeField, Min(1)] int startingLives = 10;
        [SerializeField, Min(0)] int startingCurrency = 50;
        [Tooltip("Auto Wave's state when the level (re)starts.")]
        [SerializeField] bool autoWaveByDefault;
        [Tooltip("Seconds between a wave resolving and the next one starting while Auto Wave is on.")]
        [SerializeField, Min(0.5f)] float autoWaveDelay = 3f;

        public GameSession Session { get; private set; }
        public WaveSpawner Waves => waveSpawner;
        public WaveCountdown AutoWave { get; private set; }

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
            AutoWave = new WaveCountdown(autoWaveDelay, autoWaveByDefault);
        }

        void Update()
        {
            if (AutoWave.Tick(Time.deltaTime, CanStartNextWave))
                StartNextWave();
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

        /// <summary>
        /// Starts the next wave, if the game is running and no wave is in progress. This is also Send Now:
        /// it skips whatever is left of an Auto Wave countdown.
        /// </summary>
        public bool StartNextWave()
        {
            if (!CanStartNextWave || !waveSpawner.StartNextWave())
                return false;

            AutoWave.Cancel();
            return true;
        }

        public void SetAutoWave(bool enabled)
        {
            AutoWave.SetAuto(enabled);
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
            AutoWave.Cancel();
            waveSpawner.Halt();
            GameEnded?.Invoke(Session.Outcome);
        }
    }
}
