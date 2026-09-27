using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectTD.Combat;
using ProjectTD.Core;
using ProjectTD.Enemies;
using ProjectTD.Levels;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectTD.Tests
{
    /// <summary>
    /// Plays the real development level (sped up): the wave flow, a full win with a scripted player who pays
    /// for everything with the real economy, a loss, the frozen end state, and restarting.
    /// </summary>
    public class PrototypeSceneTests : LevelSceneFixture
    {
        const float TimeScale = 8f;
        const float MaxGameSeconds = 600f;

        int spawned, killed, escaped, highestWaveStarted;

        [UnitySetUp]
        public IEnumerator CountEvents()
        {
            spawned = killed = escaped = highestWaveStarted = 0;
            game.Waves.EnemySpawned += _ => spawned++;
            game.Waves.EnemyKilled += _ => killed++;
            game.Waves.EnemyReachedEnd += _ => escaped++;
            game.Waves.WaveStarted += wave => highestWaveStarted = Mathf.Max(highestWaveStarted, wave);
            yield break;
        }

        [UnityTest]
        public IEnumerator LevelStarts_InPreparation_AndNoWaveBeginsUntilThePlayerStartsIt()
        {
            Assert.AreEqual(GamePhase.Preparation, game.Phase);
            Assert.AreEqual(0, game.Waves.CurrentWaveNumber);
            Assert.AreEqual(50, game.Session.Currency);
            Assert.AreEqual(10, game.Session.Lives);
            Assert.IsEmpty(builder.Towers);
            Assert.IsEmpty(Object.FindObjectsByType<Tower>(), "No towers are pre-placed.");

            Time.timeScale = TimeScale;
            yield return new WaitForSeconds(20f);

            Assert.AreEqual(0, spawned, "Nothing spawns while the player is still preparing.");
            Assert.AreEqual(GamePhase.Preparation, game.Phase);
            Assert.AreEqual("Wave: - / 5", HudText("TopBar/WaveText"));

            Assert.IsTrue(game.StartNextWave());
            Assert.AreEqual(GamePhase.WaveRunning, game.Phase);
            Assert.AreEqual(1, game.Waves.CurrentWaveNumber);
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(1, spawned);
        }

        [UnityTest]
        public IEnumerator NextWave_DoesNotStartAutomatically_AndCannotStartWhileAWaveIsRunning()
        {
            game.Session.AddCurrency(200); // test setup: enough defence to clear wave 1 cleanly
            Tower a = Place(InsideHorseshoeSpot);
            Place(HorseshoeTopSpot);
            builder.TryUpgrade(a);

            Time.timeScale = TimeScale;
            Assert.IsTrue(game.StartNextWave());
            Assert.IsFalse(game.StartNextWave(), "A second wave cannot start while one is running.");
            Assert.AreEqual(1, game.Waves.CurrentWaveNumber);

            yield return WaitFor(() => game.Phase == GamePhase.Preparation, MaxGameSeconds);
            Assert.AreEqual(1, game.Waves.CurrentWaveNumber);
            int spawnedAfterWave1 = spawned;

            yield return new WaitForSeconds(30f);
            Assert.AreEqual(GamePhase.Preparation, game.Phase, "The next wave waits for the player.");
            Assert.AreEqual(1, game.Waves.CurrentWaveNumber);
            Assert.AreEqual(spawnedAfterWave1, spawned);
            Assert.AreEqual("Start Wave 2", HudText("BottomBar/StartWaveButton/Label"));

            Assert.IsTrue(game.StartNextWave());
            Assert.AreEqual(2, game.Waves.CurrentWaveNumber);
        }

        [UnityTest]
        public IEnumerator Enemies_FollowTheSmoothRoad_AndReachTheEndExactlyOnce()
        {
            Time.timeScale = TimeScale;
            game.StartNextWave();
            yield return WaitFor(() => game.Waves.ActiveEnemies.Count > 0, 5f);

            Enemy enemy = game.Waves.ActiveEnemies[0];
            int reachedEnd = 0;
            enemy.ReachedEnd += _ => reachedEnd++;
            var positions = new List<Vector2>();
            float lastProgress = 0f;
            while (enemy != null)
            {
                Assert.GreaterOrEqual(enemy.PathProgress, lastProgress, "Progress along the road never decreases.");
                Assert.Less(path.DistanceTo(enemy.Position), 1e-3f, "The enemy is on the road's centre line.");
                lastProgress = enemy.PathProgress;
                if (positions.Count == 0 || positions[positions.Count - 1] != enemy.Position)
                    positions.Add(enemy.Position);
                yield return null;
            }

            Assert.AreEqual(1, reachedEnd);
            Assert.AreEqual(path.Length, lastProgress, 0.05f, "It walked the whole road.");
            Assert.AreEqual(9, game.Session.Lives);

            float maxTurn = 0f;
            for (int i = 2; i < positions.Count; i++)
                maxTurn = Mathf.Max(maxTurn, Vector2.Angle(positions[i - 1] - positions[i - 2], positions[i] - positions[i - 1]));
            TestContext.WriteLine($"Road length {path.Length:0.0}, {positions.Count} frames, largest turn between frames {maxTurn:0.0} degrees.");
            Assert.Less(maxTurn, 25f, "No sudden changes of direction.");
        }

        [UnityTest]
        public IEnumerator WithAScriptedDefense_BoughtWithTheRealEconomy_ThePlayerWinsAllWaves()
        {
            // The scripted player follows a fixed build order, doing each step as soon as it can afford it.
            var plan = new List<(Vector2? spot, int upgradeIndex)>
            {
                (InsideHorseshoeSpot, -1), (RidgeBendSpot, -1), (null, 0), (null, 1), (HorseshoeTopSpot, -1), (null, 0), (null, 2),
                (null, 1), (WestBankSpot, -1), (null, 0), (null, 2), (null, 1), (null, 3), (null, 3), (null, 2), (null, 3),
            };
            var towers = new List<Tower>();
            int step = 0, spent = 0;
            bool sawDamagedEnemy = false, sawProjectile = false;

            Time.timeScale = TimeScale;
            float deadline = Time.time + MaxGameSeconds;
            while (!game.Session.IsOver && Time.time < deadline)
            {
                if (game.Phase == GamePhase.Preparation)
                {
                    while (step < plan.Count)
                    {
                        int before = game.Session.Currency;
                        (Vector2? spot, int upgradeIndex) = plan[step];
                        bool done = spot.HasValue
                            ? AddIfPlaced(towers, builder.TryPlace(towerPrefab, spot.Value))
                            : builder.TryUpgrade(towers[upgradeIndex]);
                        if (!done)
                            break;
                        spent += before - game.Session.Currency;
                        step++;
                    }
                    TestContext.WriteLine($"Before wave {game.Waves.CurrentWaveNumber + 1}: lives {game.Session.Lives}, gold {game.Session.Currency}, {towers.Count} towers, plan step {step}.");
                    Assert.IsTrue(game.StartNextWave());
                }

                sawProjectile |= Object.FindAnyObjectByType<Projectile>() != null;
                foreach (Enemy enemy in game.Waves.ActiveEnemies)
                    sawDamagedEnemy |= enemy.IsAlive && enemy.Health < enemy.MaxHealth;
                yield return null;

                if (game.Session.Outcome == GameOutcome.Victory)
                    Assert.AreEqual(0, game.Waves.ActiveEnemies.Count, "Victory only once every enemy of the final wave is resolved.");
            }

            yield return null; // let the HUD catch up with the frame the game ended in
            TestContext.WriteLine($"Outcome {game.Session.Outcome}: spawned {spawned}, killed {killed}, escaped {escaped}, lives {game.Session.Lives}, gold {game.Session.Currency}, spent {spent}.");
            Assert.AreEqual(GameOutcome.Victory, game.Session.Outcome);
            Assert.AreEqual(GamePhase.Victory, game.Phase);
            Assert.AreEqual(game.Waves.WaveCount, highestWaveStarted, "Every wave started.");
            Assert.IsTrue(sawProjectile && sawDamagedEnemy, "Towers fire projectiles that damage enemies.");
            Assert.AreEqual(spawned, killed + escaped, "Every spawned enemy was resolved exactly once.");
            Assert.AreEqual(50 - spent + killed * 5, game.Session.Currency, "Currency = start - purchases + one reward per kill.");
            Assert.AreEqual(10 - escaped, game.Session.Lives, "Each escaped enemy cost exactly one life.");
            Assert.AreEqual("VICTORY", HudText("EndPanel/Box/OutcomeText"));

            yield return AssertGameIsFrozen(towers);
        }

        [UnityTest]
        public IEnumerator WithoutTowers_EnemiesEscape_AndThePlayerLoses_ThenEverythingStops()
        {
            Time.timeScale = TimeScale;
            float deadline = Time.time + MaxGameSeconds;
            while (!game.Session.IsOver && Time.time < deadline)
            {
                if (game.Phase == GamePhase.Preparation)
                    game.StartNextWave();
                yield return null;
            }

            yield return null; // let the HUD catch up with the frame the game ended in
            Assert.AreEqual(GameOutcome.Defeat, game.Session.Outcome);
            Assert.AreEqual(0, game.Session.Lives);
            Assert.AreEqual(10, escaped, "Lives were lost one per escaped enemy, down to zero.");
            Assert.AreEqual(0, killed);
            Assert.AreEqual(50, game.Session.Currency);
            Assert.AreEqual("DEFEAT", HudText("EndPanel/Box/OutcomeText"));
            Assert.Greater(game.Waves.ActiveEnemies.Count, 0, "Some enemies were still on the road at the moment of defeat.");

            var frozenAt = new List<Vector2>();
            foreach (Enemy enemy in game.Waves.ActiveEnemies)
                frozenAt.Add(enemy.Position);
            int spawnedAtDefeat = spawned;

            yield return AssertGameIsFrozen(new List<Tower>());

            Assert.AreEqual(spawnedAtDefeat, spawned, "No enemies spawn after defeat.");
            for (int i = 0; i < frozenAt.Count; i++)
                Assert.AreEqual(frozenAt[i], game.Waves.ActiveEnemies[i].Position, "Enemies stop moving at defeat.");
            Assert.AreEqual(10, escaped, "No further enemies escape.");
        }

        [UnityTest]
        public IEnumerator Restart_RestoresACleanLevel_MidWave()
        {
            Place(InsideHorseshoeSpot);
            Time.timeScale = TimeScale;
            game.StartNextWave();
            yield return WaitFor(() => game.Waves.ActiveEnemies.Count >= 3, 30f);
            GameController oldGame = game;

            game.Restart();
            yield return WaitFor(() => Object.FindAnyObjectByType<GameController>() != null && Object.FindAnyObjectByType<GameController>() != oldGame, 10f);
            yield return null;
            FindLevelObjects();

            Assert.AreEqual(50, game.Session.Currency);
            Assert.AreEqual(10, game.Session.Lives);
            Assert.AreEqual(0, game.Waves.CurrentWaveNumber);
            Assert.AreEqual(GamePhase.Preparation, game.Phase);
            Assert.IsEmpty(builder.Towers);
            Assert.IsEmpty(Object.FindObjectsByType<Tower>());
            Assert.IsEmpty(Object.FindObjectsByType<Enemy>());
            Assert.AreEqual("Lives: 10", HudText("TopBar/LivesText"));
            Assert.AreEqual("Gold: 50", HudText("TopBar/CurrencyText"));
            Assert.AreEqual("Wave: - / 5", HudText("TopBar/WaveText"));
            Assert.IsNull(GameObject.Find("HUD/EndPanel"), "No end screen.");

            Assert.IsNotNull(builder.TryPlace(towerPrefab, InsideHorseshoeSpot), "The restarted level is playable.");
            Assert.IsTrue(game.StartNextWave());
        }

        [UnityTest]
        public IEnumerator Restart_AfterDefeat_RestoresACleanLevel()
        {
            game.Session.LoseLives(9);
            Time.timeScale = TimeScale;
            game.StartNextWave();
            yield return WaitFor(() => game.Session.IsOver, MaxGameSeconds);
            yield return null;
            Assert.IsNotNull(GameObject.Find("HUD/EndPanel/Box/RestartLevelButton"), "The end screen offers a restart.");

            GameController oldGame = game;
            game.Restart();
            yield return WaitFor(() => Object.FindAnyObjectByType<GameController>() != null && Object.FindAnyObjectByType<GameController>() != oldGame, 10f);
            yield return null;
            FindLevelObjects();

            Assert.AreEqual(GamePhase.Preparation, game.Phase);
            Assert.AreEqual(10, game.Session.Lives);
            Assert.AreEqual(50, game.Session.Currency);
            Assert.IsEmpty(Object.FindObjectsByType<Enemy>());
            Assert.IsNull(GameObject.Find("HUD/EndPanel"));
        }

        IEnumerator AssertGameIsFrozen(List<Tower> towers)
        {
            int currency = game.Session.Currency;
            int lives = game.Session.Lives;
            var shots = new List<int>();
            foreach (Tower tower in towers)
            {
                Assert.IsFalse(tower.enabled, "Towers stop when the game ends.");
                shots.Add(tower.ShotsFired);
            }

            Assert.IsFalse(game.StartNextWave());
            Assert.IsNull(builder.TryPlace(towerPrefab, NearSpawnSpot), "No building after the game ends.");
            if (towers.Count > 0)
            {
                Assert.IsFalse(builder.TryUpgrade(towers[0]), "No upgrades after the game ends.");
                Assert.IsFalse(builder.TrySell(towers[0]), "No selling after the game ends.");
            }
            interaction.BeginPlacement(towerPrefab);
            Assert.IsFalse(interaction.IsPlacing, "Placement mode cannot start after the game ends.");

            yield return new WaitForSeconds(20f);

            Assert.AreEqual(currency, game.Session.Currency, "Currency no longer changes.");
            Assert.AreEqual(lives, game.Session.Lives, "Lives no longer change.");
            for (int i = 0; i < towers.Count; i++)
                Assert.AreEqual(shots[i], towers[i].ShotsFired, "Towers no longer fire.");
            Assert.AreEqual(towers.Count, builder.Towers.Count);
        }

        static bool AddIfPlaced(List<Tower> towers, Tower tower)
        {
            if (tower != null)
                towers.Add(tower);
            return tower != null;
        }
    }
}
