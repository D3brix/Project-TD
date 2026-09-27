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
    /// Runs the waves in order: spawns each wave's enemies at the path start, waits until the wave
    /// is cleared, pauses, then starts the next. Also owns the list of living enemies that towers target.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] WaypointPath path;
        [SerializeField] Enemy enemyPrefab;
        [SerializeField, Min(0f)] float firstWaveDelay = 2f;
        [SerializeField, Min(0f)] float timeBetweenWaves = 3f;
        [SerializeField] WaveDefinition[] waves = { new WaveDefinition() };

        readonly List<Enemy> activeEnemies = new List<Enemy>();
        Vector2[] waypoints;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;
        public event Action AllWavesCompleted;
        public event Action<Enemy> EnemySpawned;
        public event Action<Enemy> EnemyKilled;
        public event Action<Enemy> EnemyReachedEnd;

        public WaveProgress Progress { get; private set; }
        public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;
        public int WaveCount => waves.Length;

        /// <summary>One-based number of the current wave; 0 before the first wave starts.</summary>
        public int CurrentWaveNumber => Progress.CurrentWaveIndex + 1;

        void Awake()
        {
            Progress = new WaveProgress(waves.Length);
            waypoints = path.GetPoints();
        }

        void Start()
        {
            StartCoroutine(RunWaves());
        }

        /// <summary>Stops spawning further enemies. Enemies already on the map keep going.</summary>
        public void StopSpawning()
        {
            StopAllCoroutines();
        }

        IEnumerator RunWaves()
        {
            yield return new WaitForSeconds(firstWaveDelay);

            foreach (WaveDefinition wave in waves)
            {
                Progress.BeginWave();
                WaveStarted?.Invoke(CurrentWaveNumber);

                for (int i = 0; i < wave.enemyCount; i++)
                {
                    if (i > 0)
                        yield return new WaitForSeconds(wave.spawnInterval);
                    Spawn(wave);
                }

                Progress.FinishSpawning();
                yield return new WaitUntil(() => Progress.IsCurrentWaveComplete);
                WaveCompleted?.Invoke(CurrentWaveNumber);

                if (Progress.AreAllWavesComplete)
                {
                    AllWavesCompleted?.Invoke();
                    yield break;
                }

                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        void Spawn(WaveDefinition wave)
        {
            Enemy enemy = Instantiate(enemyPrefab, waypoints[0], Quaternion.identity, transform);
            enemy.Initialize(waypoints, wave.healthMultiplier);
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
        }

        void HandleEnemyReachedEnd(Enemy enemy)
        {
            Remove(enemy);
            EnemyReachedEnd?.Invoke(enemy);
        }

        void Remove(Enemy enemy)
        {
            enemy.Killed -= HandleEnemyKilled;
            enemy.ReachedEnd -= HandleEnemyReachedEnd;
            activeEnemies.Remove(enemy);
            Progress.EnemyRemoved();
        }
    }
}
