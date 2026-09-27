using NUnit.Framework;
using ProjectTD.Core;
using ProjectTD.Waves;

namespace ProjectTD.Tests
{
    public class GameSessionTests
    {
        [Test]
        public void StartsWithConfiguredLivesAndCurrency()
        {
            var session = new GameSession(10, 50);

            Assert.AreEqual(10, session.Lives);
            Assert.AreEqual(50, session.Currency);
            Assert.AreEqual(GameOutcome.InProgress, session.Outcome);
        }

        [Test]
        public void AddCurrency_AddsReward()
        {
            var session = new GameSession(10, 50);

            session.AddCurrency(5);

            Assert.AreEqual(55, session.Currency);
        }

        [Test]
        public void LoseLives_DeductsExactlyTheGivenAmount()
        {
            var session = new GameSession(10, 0);

            session.LoseLives(1);

            Assert.AreEqual(9, session.Lives);
        }

        [Test]
        public void LosingLastLife_IsDefeat_AndLivesNeverGoNegative()
        {
            var session = new GameSession(2, 0);

            session.LoseLives(1);
            Assert.AreEqual(GameOutcome.InProgress, session.Outcome);

            session.LoseLives(5);
            Assert.AreEqual(0, session.Lives);
            Assert.AreEqual(GameOutcome.Defeat, session.Outcome);
        }

        [Test]
        public void AfterDefeat_LivesAndCurrencyNoLongerChange()
        {
            var session = new GameSession(1, 10);
            session.LoseLives(1);

            session.LoseLives(1);
            session.AddCurrency(5);

            Assert.AreEqual(0, session.Lives);
            Assert.AreEqual(10, session.Currency);
        }

        [Test]
        public void TrySpend_DeductsExactlyTheAmount_WhenAffordable()
        {
            var session = new GameSession(10, 50);

            Assert.IsTrue(session.TrySpend(30));
            Assert.AreEqual(20, session.Currency);
        }

        [Test]
        public void TrySpend_IsRefused_AndChangesNothing_WhenUnaffordable()
        {
            var session = new GameSession(10, 20);

            Assert.IsFalse(session.CanAfford(30));
            Assert.IsFalse(session.TrySpend(30));
            Assert.AreEqual(20, session.Currency);

            Assert.IsTrue(session.TrySpend(20), "Spending exactly everything is allowed.");
            Assert.AreEqual(0, session.Currency);
        }

        [Test]
        public void AfterTheGameEnds_NothingCanBeSpent()
        {
            var session = new GameSession(1, 100);
            session.LoseLives(1);

            Assert.IsFalse(session.CanAfford(10));
            Assert.IsFalse(session.TrySpend(10));
            Assert.AreEqual(100, session.Currency);
        }

        [Test]
        public void Victory_IsRefused_WhileEnemiesOfTheFinalWaveRemain()
        {
            var session = new GameSession(10, 0);
            var waves = new WaveProgress(1);
            waves.BeginWave();
            waves.EnemySpawned();
            waves.EnemySpawned();
            waves.FinishSpawning();
            waves.EnemyRemoved();

            Assert.IsFalse(session.TryDeclareVictory(waves));
            Assert.AreEqual(GameOutcome.InProgress, session.Outcome);
        }

        [Test]
        public void Victory_IsRefused_WhileFinalWaveIsStillSpawning()
        {
            var session = new GameSession(10, 0);
            var waves = new WaveProgress(1);
            waves.BeginWave();

            Assert.IsFalse(session.TryDeclareVictory(waves));
        }

        [Test]
        public void Victory_WhenAllWavesAreCleared()
        {
            var session = new GameSession(10, 0);
            var waves = ClearedSingleWave();

            Assert.IsTrue(session.TryDeclareVictory(waves));
            Assert.AreEqual(GameOutcome.Victory, session.Outcome);
            Assert.IsFalse(session.TryDeclareVictory(waves), "Victory is declared only once.");
        }

        [Test]
        public void Victory_IsRefused_AfterDefeat()
        {
            var session = new GameSession(1, 0);
            session.LoseLives(1);

            Assert.IsFalse(session.TryDeclareVictory(ClearedSingleWave()));
            Assert.AreEqual(GameOutcome.Defeat, session.Outcome);
        }

        static WaveProgress ClearedSingleWave()
        {
            var waves = new WaveProgress(1);
            waves.BeginWave();
            waves.EnemySpawned();
            waves.FinishSpawning();
            waves.EnemyRemoved();
            return waves;
        }
    }
}
