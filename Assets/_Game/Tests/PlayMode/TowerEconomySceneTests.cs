using System.Collections;
using NUnit.Framework;
using ProjectTD.Combat;
using ProjectTD.Core;
using ProjectTD.Enemies;
using ProjectTD.Placement;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.TestTools;

namespace ProjectTD.Tests
{
    /// <summary>Buying, placing, upgrading and selling towers on the real level, with its real prices.</summary>
    public class TowerEconomySceneTests : LevelSceneFixture
    {
        [Test]
        public void TheStartingGold_BuysTwoTowers_OrOneTowerAndAnUpgrade()
        {
            Assert.AreEqual(25, towerPrefab.BuildCost);
            Assert.AreEqual(50, game.Session.Currency);
            Assert.IsTrue(builder.CanAfford(towerPrefab));
            Assert.AreEqual(2, game.Session.Currency / towerPrefab.BuildCost,
                "The opening buys two towers, or one tower and its first upgrade: a choice, not 'build everywhere'.");
            Assert.LessOrEqual(towerPrefab.BuildCost + towerPrefab.Progression.Next.cost, game.Session.Currency);
        }

        [UnityTest]
        public IEnumerator ValidPurchase_DeductsTheCostOnce_AndTheTowerBecomesActive()
        {
            Tower tower = builder.TryPlace(towerPrefab, InsideHorseshoeSpot);

            Assert.IsNotNull(tower);
            Assert.AreEqual(25, game.Session.Currency);
            Assert.AreEqual(1, builder.Towers.Count);
            Assert.AreEqual((Vector3)(Vector2)InsideHorseshoeSpot, tower.transform.position);

            yield return null;
            Assert.AreEqual(25, game.Session.Currency, "Nothing else is charged afterwards.");

            Time.timeScale = 8f;
            game.StartNextWave();
            yield return WaitFor(() => tower.ShotsFired > 0, 60f);
        }

        [Test]
        public void InvalidPlacements_AreRejected_AndCostNothing()
        {
            Place(InsideHorseshoeSpot);
            Assert.AreEqual(25, game.Session.Currency);

            Assert.AreEqual(PlacementResult.OnRoad, builder.CheckPlacement(towerPrefab, path.Points[200]));
            Assert.IsNull(builder.TryPlace(towerPrefab, path.Points[200]), "On the road.");
            Assert.AreEqual(PlacementResult.OverlapsTower, builder.CheckPlacement(towerPrefab, InsideHorseshoeSpot + new Vector2(0f, 0.35f)));
            Assert.IsNull(builder.TryPlace(towerPrefab, InsideHorseshoeSpot + new Vector2(0f, 0.35f)), "On top of another tower.");
            Assert.AreEqual(PlacementResult.OutOfBounds, builder.CheckPlacement(towerPrefab, new Vector2(14.5f, 0f)));
            Assert.IsNull(builder.TryPlace(towerPrefab, new Vector2(14.5f, 0f)), "Outside the battlefield.");
            Assert.AreEqual(PlacementResult.BlockedTerrain, builder.CheckPlacement(towerPrefab, new Vector2(2.5f, -3.4f)));
            Assert.IsNull(builder.TryPlace(towerPrefab, new Vector2(2.5f, -3.4f)), "In the pond.");

            Assert.AreEqual(25, game.Session.Currency);
            Assert.AreEqual(1, builder.Towers.Count);
        }

        [UnityTest]
        public IEnumerator CancelledPlacement_CostsNothing_AndLeavesNoGhost()
        {
            interaction.BeginPlacement(towerPrefab);
            Assert.IsTrue(interaction.IsPlacing);
            interaction.PreviewAt(InsideHorseshoeSpot);
            Assert.AreEqual(PlacementResult.Valid, interaction.PlacementState);
            interaction.PreviewAt(path.Points[100]);
            Assert.AreEqual(PlacementResult.OnRoad, interaction.PlacementState, "The preview reports the road as invalid.");

            interaction.CancelPlacement();
            yield return null;

            Assert.IsFalse(interaction.IsPlacing);
            Assert.AreEqual(50, game.Session.Currency);
            Assert.IsEmpty(builder.Towers);
            Assert.IsEmpty(Object.FindObjectsByType<Tower>(), "The ghost is gone.");
        }

        [UnityTest]
        public IEnumerator ConfirmingAnInvalidSpot_KeepsPlacing_AndCostsNothing_ThenAValidSpotBuilds()
        {
            interaction.BeginPlacement(towerPrefab);

            Assert.IsNull(interaction.ConfirmPlacement(path.Points[100]));
            Assert.IsTrue(interaction.IsPlacing, "Still placing after an invalid click.");
            Assert.AreEqual(50, game.Session.Currency);

            Tower tower = interaction.ConfirmPlacement(InsideHorseshoeSpot);
            yield return null;

            Assert.IsNotNull(tower);
            Assert.IsFalse(interaction.IsPlacing);
            Assert.AreEqual(25, game.Session.Currency);
            Assert.AreEqual(1, Object.FindObjectsByType<Tower>().Length, "Exactly the built tower; the ghost is gone.");
        }

        [Test]
        public void UnaffordableTower_CannotBePlaced_NorCanPlacementStart()
        {
            Place(InsideHorseshoeSpot);
            Place(HorseshoeTopSpot);
            Assert.AreEqual(0, game.Session.Currency);

            Assert.IsFalse(builder.CanAfford(towerPrefab));
            Assert.AreEqual(PlacementResult.NotEnoughCurrency, builder.CheckPlacement(towerPrefab, RidgeBendSpot));
            Assert.IsNull(builder.TryPlace(towerPrefab, RidgeBendSpot));
            interaction.BeginPlacement(towerPrefab);
            Assert.IsFalse(interaction.IsPlacing);
            Assert.AreEqual(0, game.Session.Currency);
            Assert.AreEqual(2, builder.Towers.Count);
        }

        [Test]
        public void Upgrade_DeductsItsCost_AndChangesTheTowersStats()
        {
            Tower tower = Place(InsideHorseshoeSpot);
            TowerLevel next = tower.Progression.Next;

            Assert.IsTrue(builder.TryUpgrade(tower));

            Assert.AreEqual(25 - next.cost, game.Session.Currency);
            Assert.AreEqual(1, tower.Progression.LevelIndex);
            Assert.AreEqual(next.damage, tower.Damage);
            Assert.AreEqual(next.attackInterval, tower.AttackInterval);
            Assert.AreEqual(next.range, tower.Range);
        }

        [Test]
        public void UnaffordableUpgrade_IsRefused_AndCostsNothing()
        {
            Tower tower = Place(InsideHorseshoeSpot);
            builder.TryUpgrade(tower); // 25 -> 5, and the next upgrade costs more than that

            Assert.AreEqual(5, game.Session.Currency);
            Assert.IsFalse(builder.CanUpgrade(tower));
            Assert.IsFalse(builder.TryUpgrade(tower));
            Assert.AreEqual(1, tower.Progression.LevelIndex);
        }

        [Test]
        public void EveryUpgradeStep_ChargesItsOwnCost_AndMaxLevelCannotBeBoughtAgain()
        {
            game.Session.AddCurrency(500); // test setup
            Tower tower = Place(InsideHorseshoeSpot);
            int currency = game.Session.Currency;

            while (!tower.Progression.IsMaxLevel)
            {
                int cost = tower.Progression.Next.cost;
                Assert.IsTrue(builder.TryUpgrade(tower));
                Assert.AreEqual(currency - cost, game.Session.Currency);
                currency = game.Session.Currency;
            }

            Assert.AreEqual(4, tower.Progression.LevelCount);
            Assert.IsFalse(builder.CanUpgrade(tower));
            Assert.IsFalse(builder.TryUpgrade(tower));
            Assert.AreEqual(currency, game.Session.Currency);
            Assert.AreEqual(6f, tower.Damage);
            Assert.AreEqual(0.45f, tower.AttackInterval);
            Assert.AreEqual(3.6f, tower.Range);
        }

        [UnityTest]
        public IEnumerator Upgrade_ChangesTheDamageActuallyDealt()
        {
            Tower tower = Place(NearSpawnSpot);
            Assert.IsTrue(builder.TryUpgrade(tower)); // Heavy Bolts: 3 -> 4.5 damage per hit
            Assert.AreEqual(4.5f, tower.Damage);

            Time.timeScale = 4f;
            game.StartNextWave();
            Enemy hit = null;
            yield return WaitFor(() =>
            {
                foreach (Enemy enemy in game.Waves.ActiveEnemies)
                    if (enemy.Health < enemy.MaxHealth)
                        hit = enemy;
                return hit != null;
            }, 30f);

            Assert.AreEqual(4.5f, hit.MaxHealth - hit.Health, 1e-4f, "The first hit dealt the upgraded damage.");
        }

        [UnityTest]
        public IEnumerator Selling_RefundsOnce_AndRemovesTheTower()
        {
            Tower tower = Place(InsideHorseshoeSpot);
            builder.TryUpgrade(tower);
            Assert.AreEqual(5, game.Session.Currency);
            int refund = builder.SellValue(tower);
            Assert.AreEqual((25 + 20) * 70 / 100, refund, "70% of everything invested, rounded down.");

            interaction.Select(tower);
            Assert.IsTrue(builder.TrySell(tower));
            Assert.AreEqual(5 + refund, game.Session.Currency);
            Assert.IsFalse(builder.TrySell(tower), "A tower can only be sold once.");
            Assert.AreEqual(5 + refund, game.Session.Currency);

            yield return null;
            Assert.IsTrue(tower == null, "The sold tower is removed.");
            Assert.IsEmpty(builder.Towers);
            Assert.IsNull(interaction.Selected, "Selling clears the selection.");
            Assert.AreEqual(PlacementResult.Valid, builder.CheckPlacement(towerPrefab, InsideHorseshoeSpot), "Its spot is free again.");
        }

        [UnityTest]
        public IEnumerator SoldTower_StopsActing_WhileEnemiesAndProjectilesCarryOn()
        {
            Tower tower = Place(NearSpawnSpot);
            Time.timeScale = 4f;
            game.StartNextWave();
            yield return WaitFor(() => tower.ShotsFired >= 2, 30f);
            int shots = tower.ShotsFired;

            Assert.IsTrue(builder.TrySell(tower));
            Assert.AreEqual(shots, tower.ShotsFired, "No shot after the sale.");

            yield return new WaitForSeconds(6f); // longer than any projectile lives
            Assert.IsTrue(tower == null, "The sold tower is gone.");
            Assert.IsNull(Object.FindAnyObjectByType<Projectile>(), "No new projectiles appear.");
            Assert.AreEqual(GamePhase.WaveRunning, game.Phase, "The wave carries on undisturbed.");
        }
    }
}
