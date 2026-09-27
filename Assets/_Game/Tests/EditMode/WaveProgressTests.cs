using System;
using NUnit.Framework;
using ProjectTD.Waves;

namespace ProjectTD.Tests
{
    public class WaveProgressTests
    {
        [Test]
        public void BeforeFirstWave_NothingIsComplete()
        {
            var waves = new WaveProgress(2);

            Assert.IsFalse(waves.HasStarted);
            Assert.IsFalse(waves.IsCurrentWaveComplete);
            Assert.IsFalse(waves.AreAllWavesComplete);
        }

        [Test]
        public void Wave_IsNotComplete_WhileStillSpawning_EvenWithNoLivingEnemies()
        {
            var waves = new WaveProgress(2);
            waves.BeginWave();
            waves.EnemySpawned();
            waves.EnemyRemoved(); // first enemy dies before the second has spawned

            Assert.AreEqual(0, waves.ActiveEnemies);
            Assert.IsFalse(waves.IsCurrentWaveComplete);
        }

        [Test]
        public void Wave_IsNotComplete_WhileEnemiesAreAlive()
        {
            var waves = new WaveProgress(2);
            waves.BeginWave();
            waves.EnemySpawned();
            waves.EnemySpawned();
            waves.FinishSpawning();
            waves.EnemyRemoved();

            Assert.IsFalse(waves.IsCurrentWaveComplete);

            waves.EnemyRemoved();
            Assert.IsTrue(waves.IsCurrentWaveComplete);
        }

        [Test]
        public void AllWavesComplete_OnlyAfterFinalWaveIsCleared()
        {
            var waves = new WaveProgress(2);

            ClearWave(waves, enemies: 3);
            Assert.IsTrue(waves.IsCurrentWaveComplete);
            Assert.IsFalse(waves.AreAllWavesComplete, "Only the first of two waves is done.");

            waves.BeginWave();
            waves.EnemySpawned();
            waves.FinishSpawning();
            Assert.IsFalse(waves.AreAllWavesComplete, "An enemy of the final wave is still alive.");

            waves.EnemyRemoved();
            Assert.IsTrue(waves.AreAllWavesComplete);
        }

        [Test]
        public void NextWave_CannotBegin_BeforeCurrentWaveIsComplete()
        {
            var waves = new WaveProgress(2);
            waves.BeginWave();
            waves.EnemySpawned();
            waves.FinishSpawning();

            Assert.Throws<InvalidOperationException>(() => waves.BeginWave());
        }

        [Test]
        public void CannotBeginMoreWavesThanExist()
        {
            var waves = new WaveProgress(1);
            ClearWave(waves, enemies: 1);

            Assert.IsFalse(waves.HasMoreWaves);
            Assert.Throws<InvalidOperationException>(() => waves.BeginWave());
        }

        [Test]
        public void RemovingMoreEnemiesThanSpawned_Throws()
        {
            var waves = new WaveProgress(1);
            waves.BeginWave();

            Assert.Throws<InvalidOperationException>(() => waves.EnemyRemoved());
        }

        static void ClearWave(WaveProgress waves, int enemies)
        {
            waves.BeginWave();
            for (int i = 0; i < enemies; i++)
                waves.EnemySpawned();
            waves.FinishSpawning();
            for (int i = 0; i < enemies; i++)
                waves.EnemyRemoved();
        }
    }
}
