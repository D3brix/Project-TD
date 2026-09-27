using System.Collections;
using NUnit.Framework;
using ProjectTD.Combat;
using ProjectTD.Core;
using ProjectTD.Enemies;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectTD.Tests
{
    /// <summary>
    /// Plays the real Phase 1 development scene (sped up) from start to finish.
    /// Any error or exception logged during play fails these tests.
    /// </summary>
    public class PrototypeSceneTests
    {
        const string SceneName = "Prototype";
        const float TimeScale = 8f;
        const float MaxGameSeconds = 400f;

        GameController game;
        int spawned, killed, escaped, highestWaveStarted;
        int startingLives, startingCurrency;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);

            game = Object.FindAnyObjectByType<GameController>();
            Assert.IsNotNull(game, "The scene has a GameController.");

            spawned = killed = escaped = highestWaveStarted = 0;
            startingLives = game.Session.Lives;
            startingCurrency = game.Session.Currency;
            game.Waves.EnemySpawned += _ => spawned++;
            game.Waves.EnemyKilled += _ => killed++;
            game.Waves.EnemyReachedEnd += _ => escaped++;
            game.Waves.WaveStarted += wave => highestWaveStarted = Mathf.Max(highestWaveStarted, wave);

            Time.timeScale = TimeScale;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator WithTowers_ThePlayerWinsAllWaves()
        {
            Tower[] towers = Object.FindObjectsByType<Tower>();
            Assert.IsNotEmpty(towers);

            bool sawEnemyMove = false, sawTargetAcquired = false, sawProjectile = false, sawDamagedEnemy = false;
            Enemy tracked = null;
            Vector2 trackedStart = default;

            float deadline = Time.time + MaxGameSeconds;
            while (!game.Session.IsOver && Time.time < deadline)
            {
                if (tracked == null && game.Waves.ActiveEnemies.Count > 0)
                {
                    tracked = game.Waves.ActiveEnemies[0];
                    trackedStart = tracked.Position;
                }
                else if (tracked != null && tracked.IsAlive && tracked.Position != trackedStart)
                {
                    sawEnemyMove = true;
                }

                foreach (Tower tower in towers)
                    sawTargetAcquired |= tower.CurrentTarget != null;
                sawProjectile |= Object.FindAnyObjectByType<Projectile>() != null;
                foreach (Enemy enemy in game.Waves.ActiveEnemies)
                    sawDamagedEnemy |= enemy.IsAlive && enemy.Health < enemy.MaxHealth;

                yield return null;
            }

            TestContext.WriteLine($"Outcome {game.Session.Outcome}: spawned {spawned}, killed {killed}, escaped {escaped}, " +
                                  $"lives {game.Session.Lives}, currency {game.Session.Currency}.");
            Assert.AreEqual(GameOutcome.Victory, game.Session.Outcome,
                $"Expected victory. Lives {game.Session.Lives}, wave {game.Waves.CurrentWaveNumber}, killed {killed}, escaped {escaped}.");

            Assert.IsTrue(sawEnemyMove, "Enemies move along the path.");
            Assert.IsTrue(sawTargetAcquired, "Towers acquire targets.");
            Assert.IsTrue(sawProjectile, "Towers fire visible projectiles.");
            Assert.IsTrue(sawDamagedEnemy, "Enemies take damage without dying immediately.");
            foreach (Tower tower in towers)
                Assert.Greater(tower.ShotsFired, 0, $"{tower.name} fired.");

            Assert.AreEqual(game.Waves.WaveCount, highestWaveStarted, "Every wave started.");
            Assert.Greater(killed, 0);
            Assert.AreEqual(spawned, killed + escaped, "Every spawned enemy was resolved exactly once.");
            Assert.AreEqual(0, game.Waves.ActiveEnemies.Count, "No enemies remain at victory.");
            Assert.AreEqual(startingCurrency + killed * 5, game.Session.Currency, "Each kill paid its reward exactly once.");
            Assert.AreEqual(startingLives - escaped, game.Session.Lives, "Each escaped enemy cost exactly one life.");

            AssertHudShows(game.Session, "VICTORY");
        }

        [UnityTest]
        public IEnumerator WithoutTowers_EnemiesEscape_AndThePlayerLoses()
        {
            foreach (Tower tower in Object.FindObjectsByType<Tower>())
                tower.enabled = false;

            float deadline = Time.time + MaxGameSeconds;
            while (!game.Session.IsOver && Time.time < deadline)
                yield return null;

            Assert.AreEqual(GameOutcome.Defeat, game.Session.Outcome);
            Assert.AreEqual(0, game.Session.Lives);
            Assert.AreEqual(startingLives, escaped, "Lives were lost one per escaped enemy, down to zero.");
            Assert.AreEqual(0, killed);
            Assert.AreEqual(startingCurrency, game.Session.Currency);

            // Let the remaining enemies walk off: spawning has stopped and the result must not change.
            int spawnedAtDefeat = spawned;
            int waveAtDefeat = highestWaveStarted;
            yield return new WaitForSeconds(30f);

            Assert.AreEqual(spawnedAtDefeat, spawned, "No enemies spawn after defeat.");
            Assert.AreEqual(waveAtDefeat, highestWaveStarted, "No new wave starts after defeat.");
            Assert.AreEqual(GameOutcome.Defeat, game.Session.Outcome, "Defeat cannot turn into victory.");
            Assert.AreEqual(0, game.Session.Lives);

            AssertHudShows(game.Session, "DEFEAT");
        }

        static void AssertHudShows(GameSession session, string outcome)
        {
            Text outcomeText = GameObject.Find("HUD/OutcomeText")?.GetComponent<Text>();
            Assert.IsNotNull(outcomeText, "The HUD outcome text is active.");
            Assert.AreEqual(outcome, outcomeText.text);
            Assert.AreEqual($"Lives: {session.Lives}", GameObject.Find("HUD/LivesText").GetComponent<Text>().text);
            Assert.AreEqual($"Currency: {session.Currency}", GameObject.Find("HUD/CurrencyText").GetComponent<Text>().text);
            StringAssert.StartsWith("Wave: ", GameObject.Find("HUD/WaveText").GetComponent<Text>().text);
        }
    }
}
