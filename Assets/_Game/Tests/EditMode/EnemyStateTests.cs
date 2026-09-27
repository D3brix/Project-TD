using NUnit.Framework;
using ProjectTD.Enemies;

namespace ProjectTD.Tests
{
    public class EnemyStateTests
    {
        [Test]
        public void Damage_ReducesHealth()
        {
            var state = new EnemyState(10f);

            bool killed = state.ApplyDamage(3f);

            Assert.IsFalse(killed);
            Assert.AreEqual(7f, state.Health);
            Assert.IsTrue(state.IsAlive);
        }

        [Test]
        public void LethalDamage_KillsExactlyOnce()
        {
            var state = new EnemyState(10f);

            Assert.IsTrue(state.ApplyDamage(10f), "The lethal hit reports the kill.");
            Assert.IsFalse(state.ApplyDamage(10f), "Further hits must not report another kill.");
            Assert.IsFalse(state.ApplyDamage(1f));
            Assert.AreEqual(EnemyOutcome.Killed, state.Outcome);
        }

        [Test]
        public void Overkill_ClampsHealthAtZero_AndDamageAfterDeathIsIgnored()
        {
            var state = new EnemyState(10f);

            state.ApplyDamage(25f);
            state.ApplyDamage(5f);

            Assert.AreEqual(0f, state.Health);
            Assert.IsFalse(state.IsAlive);
        }

        [Test]
        public void NonPositiveDamage_IsIgnored()
        {
            var state = new EnemyState(10f);

            Assert.IsFalse(state.ApplyDamage(0f));
            Assert.IsFalse(state.ApplyDamage(-5f));
            Assert.AreEqual(10f, state.Health);
        }

        [Test]
        public void ReachEnd_HappensExactlyOnce()
        {
            var state = new EnemyState(10f);

            Assert.IsTrue(state.ReachEnd());
            Assert.IsFalse(state.ReachEnd());
            Assert.AreEqual(EnemyOutcome.ReachedEnd, state.Outcome);
        }

        [Test]
        public void KilledEnemy_CannotReachEnd()
        {
            var state = new EnemyState(10f);
            state.ApplyDamage(10f);

            Assert.IsFalse(state.ReachEnd());
            Assert.AreEqual(EnemyOutcome.Killed, state.Outcome);
        }

        [Test]
        public void EnemyThatReachedEnd_CannotBeDamagedOrKilled()
        {
            var state = new EnemyState(10f);
            state.ReachEnd();

            Assert.IsFalse(state.ApplyDamage(100f));
            Assert.AreEqual(10f, state.Health);
            Assert.AreEqual(EnemyOutcome.ReachedEnd, state.Outcome);
        }
    }
}
