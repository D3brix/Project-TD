using ProjectTD.Enemies;
using ProjectTD.Waves;
using UnityEngine;

namespace ProjectTD.Core
{
    /// <summary>
    /// Connects the waves to the session: kills pay currency, escaped enemies cost lives,
    /// and it decides victory or defeat.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [SerializeField] WaveSpawner waveSpawner;
        [SerializeField, Min(1)] int startingLives = 10;
        [SerializeField, Min(0)] int startingCurrency = 50;

        public GameSession Session { get; private set; }
        public WaveSpawner Waves => waveSpawner;

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

        void HandleEnemyKilled(Enemy enemy)
        {
            Session.AddCurrency(enemy.Reward);
        }

        void HandleEnemyReachedEnd(Enemy enemy)
        {
            Session.LoseLives(enemy.LivesCost);
            if (Session.Outcome == GameOutcome.Defeat)
                waveSpawner.StopSpawning();
        }

        void HandleAllWavesCompleted()
        {
            Session.TryDeclareVictory(waveSpawner.Progress);
        }
    }
}
