using System;
using System.Collections;
using System.Collections.Generic;
using ProjectTD.Enemies;
using ProjectTD.Levels;
using UnityEngine;

namespace ProjectTD.Waves
{
    [Serializable]
    public class WaveDefinition
    {
        [Min(1)] public int enemyCount = 5;
        [Min(0.05f)] public float spawnInterval = 1f;
        [Min(0.1f)] public float healthMultiplier = 1f;
    }

    /// <summary>
    /// Runs one wave at a time, only when asked (<see cref="StartNextWave"/>): spawns the wave's enemies at the
    /// path start and reports when the wave is cleared. Also owns the list of living enemies that towers target.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] LevelPath path;
        [SerializeField] Enemy enemyPrefab;
        [SerializeField] WaveDefinition[] waves = { new WaveDefinition() };

        readonly List<Enemy> activeEnemies = new List<Enemy>();
        bool halted;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action AllWavesCompleted;
        public event Action<Enemy> EnemySpawned;
        public event Action<Enemy> EnemyKilled;
        public event Action<Enemy> EnemyReachedEnd;

        public WaveProgress Progress { get; private set; }
        public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;
        public int WaveCount => waves.Length;

        /// <summary>One-based number of the current (or last finished) wave; 0 before the first wave starts.</summary>
        public int CurrentWaveNumber => Progress.CurrentWaveIndex + 1;

        /// <summary>A wave has started and is not cleared yet (still spawning, or enemies still alive).</summary>
        public bool IsWaveRunning => Progress.HasStarted && !Progress.IsCurrentWaveComplete;

        public bool CanStartNextWave => !halted && Progress.HasMoreWaves && !IsWaveRunning;

        void Awake()
        {
            Progress = new WaveProgress(waves.Length);
        }

        /// <summary>Starts the next wave if none is running. Returns whether a wave started.</summary>
        public bool StartNextWave()
        {
            if (!CanStartNextWave)
                return false;

            // BeginWave runs synchronously inside StartCoroutine, so a second call this frame is refused.
            StartCoroutine(RunWave(waves[Progress.CurrentWaveIndex + 1]));
            return true;
        }

        /// <summary>Stops everything for good (the game is over): no more spawns, and enemies on the map freeze.</summary>
        public void Halt()
        {
            halted = true;
            StopAllCoroutines();
            foreach (Enemy enemy in activeEnemies)
                enemy.Halt();
        }

        IEnumerator RunWave(WaveDefinition wave)
        {
            Progress.BeginWave();
            WaveStarted?.Invoke(CurrentWaveNumber);

            for (int i = 0; i < wave.enemyCount; i++)
            {
                if (i > 0)
                    yield return new WaitForSeconds(wave.spawnInterval);
                Spawn(wave);
            }

            // The last enemy was just spawned, so the wave cannot be complete yet:
            // completion is detected when its final enemy is removed.
            Progress.FinishSpawning();
        }

        void Spawn(WaveDefinition wave)
        {
            Enemy enemy = Instantiate(enemyPrefab, path.Start, Quaternion.identity, transform);
            enemy.Initialize(path.Points, wave.healthMultiplier);
            enemy.Killed += HandleEnemyKilled;
            enemy.ReachedEnd += HandleEnemyReachedEnd;

            activeEnemies.Add(enemy);
            Progress.EnemySpawned();
            EnemySpawned?.Invoke(enemy);
        }

        void HandleEnemyKilled(Enemy enemy)
        {
            Remove(enemy);
            EnemyKilled?.Invoke(enemy);
            CheckWaveComplete();
        }

        void HandleEnemyReachedEnd(Enemy enemy)
        {
            Remove(enemy);
            EnemyReachedEnd?.Invoke(enemy);
            CheckWaveComplete();
        }

        void Remove(Enemy enemy)
        {
            enemy.Killed -= HandleEnemyKilled;
            enemy.ReachedEnd -= HandleEnemyReachedEnd;
            activeEnemies.Remove(enemy);
            Progress.EnemyRemoved();
        }

        void CheckWaveComplete()
        {
            if (halted || !Progress.IsCurrentWaveComplete)
                return;

            WaveCompleted?.Invoke(CurrentWaveNumber);
            if (Progress.AreAllWavesComplete)
                AllWavesCompleted?.Invoke();
        }
    }
}
