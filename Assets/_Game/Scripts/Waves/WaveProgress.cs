using System;

namespace ProjectTD.Waves
{
    /// <summary>
    /// Counts waves and living enemies. A wave is complete only once all of its enemies
    /// have been spawned AND every one of them has been removed (killed or reached the end).
    /// Waves run one at a time: the next wave cannot begin until the current one is complete.
    /// </summary>
    public class WaveProgress
    {
        public int WaveCount { get; }

        /// <summary>Zero-based index of the current wave, or -1 before the first wave begins.</summary>
        public int CurrentWaveIndex { get; private set; } = -1;

        public int ActiveEnemies { get; private set; }
        public bool IsSpawning { get; private set; }

        public bool HasStarted => CurrentWaveIndex >= 0;
        public bool HasMoreWaves => CurrentWaveIndex < WaveCount - 1;
        public bool IsCurrentWaveComplete => HasStarted && !IsSpawning && ActiveEnemies == 0;
        public bool AreAllWavesComplete => CurrentWaveIndex == WaveCount - 1 && IsCurrentWaveComplete;

        public WaveProgress(int waveCount)
        {
            if (waveCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(waveCount), "There must be at least one wave.");

            WaveCount = waveCount;
        }

        public void BeginWave()
        {
            if (!HasMoreWaves)
                throw new InvalidOperationException("There are no more waves.");
            if (HasStarted && !IsCurrentWaveComplete)
                throw new InvalidOperationException("The current wave is not complete yet.");

            CurrentWaveIndex++;
            IsSpawning = true;
        }

        public void EnemySpawned()
        {
            if (!IsSpawning)
                throw new InvalidOperationException("Enemies can only spawn while a wave is spawning.");

            ActiveEnemies++;
        }

        public void FinishSpawning()
        {
            IsSpawning = false;
        }

        public void EnemyRemoved()
        {
            if (ActiveEnemies == 0)
                throw new InvalidOperationException("No active enemies to remove.");

            ActiveEnemies--;
        }
    }
}
