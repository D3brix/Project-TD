using System.Collections;
using System.Collections.Generic;
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
            for (int branch = 0; branch < towerPrefab.Progression.Branches.Count; branch++)
                Assert.LessOrEqual(towerPrefab.BuildCost + towerPrefab.Progression.NextIn(branch).cost, game.Session.Currency);
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
        public void TheTowerOffers_Rapid_Heavy_AndBalanced()
        {
            var branches = towerPrefab.Progression.Branches;
            Assert.AreEqual(3, branches.Count);
            Assert.AreEqual("Rapid", branches[Rapid].name);
            Assert.AreEqual("Heavy", branches[Heavy].name);
            Assert.AreEqual("Balanced", branches[Balanced].name);
            foreach (TowerBranch branch in branches)
                Assert.AreEqual(2, branch.tiers.Length);
        }

        [TestCase(Rapid)]
        [TestCase(Heavy)]
        [TestCase(Balanced)]
        public void ChoosingABranch_ChargesItsCostOnce_AppliesItsStats_AndClosesTheOtherBranches(int chosen)
        {
            game.Session.AddCurrency(500); // test setup: gold is not what stops the other branches
            Tower tower = Place(InsideHorseshoeSpot);
            int currency = game.Session.Currency;
            TowerLevel tier = tower.Progression.NextIn(chosen);

            Assert.IsTrue(builder.TryUpgrade(tower, chosen));

            Assert.AreEqual(currency - tier.cost, game.Session.Currency);
            Assert.AreEqual(chosen, tower.Progression.BranchIndex);
            Assert.AreEqual(tier.damage, tower.Damage);
            Assert.AreEqual(tier.attackInterval, tower.AttackInterval);
            Assert.AreEqual(tier.range, tower.Range);
            for (int other = 0; other < 3; other++)
            {
                if (other == chosen)
                    continue;
                Assert.IsFalse(builder.CanUpgrade(tower, other));
                Assert.IsFalse(builder.TryUpgrade(tower, other), "Committed: the other branches are closed.");
            }
            Assert.AreEqual(currency - tier.cost, game.Session.Currency, "Refused upgrades cost nothing.");
            Assert.AreEqual(tier.damage, tower.Damage);
        }

        [TestCase(Rapid)]
        [TestCase(Heavy)]
        [TestCase(Balanced)]
        public void TheSecondTier_FollowsTheBranch_AndTheLastTierCannotBeBoughtAgain(int chosen)
        {
            game.Session.AddCurrency(500); // test setup
            Tower tower = Place(InsideHorseshoeSpot);
            builder.TryUpgrade(tower, chosen);
            int currency = game.Session.Currency;
            TowerLevel second = tower.Progression.Branches[chosen].tiers[1];

            Assert.IsTrue(builder.TryUpgrade(tower, chosen));
            Assert.AreEqual(currency - second.cost, game.Session.Currency);
            Assert.AreEqual(second.damage, tower.Damage);
            Assert.AreEqual(second.attackInterval, tower.AttackInterval);
            Assert.AreEqual(second.range, tower.Range);

            Assert.IsTrue(tower.Progression.IsMaxLevel);
            for (int branch = 0; branch < 3; branch++)
                Assert.IsFalse(builder.TryUpgrade(tower, branch));
            Assert.AreEqual(currency - second.cost, game.Session.Currency);
        }

        [Test]
        public void UnaffordableUpgrades_AreRefused_AndCostNothing()
        {
            Place(InsideHorseshoeSpot);
            Tower tower = Place(HorseshoeTopSpot);
            Assert.AreEqual(0, game.Session.Currency);

            for (int branch = 0; branch < 3; branch++)
            {
                Assert.IsTrue(builder.IsUpgradeAvailable(tower, branch), "The branch is open, just not affordable.");
                Assert.IsFalse(builder.CanUpgrade(tower, branch));
                Assert.IsFalse(builder.TryUpgrade(tower, branch));
            }
            Assert.IsFalse(tower.Progression.HasBranch, "A refused purchase does not commit the tower.");
            Assert.AreEqual(0, game.Session.Currency);
        }

        [Test]
        public void TheSpecialisations_DifferAsIntended()
        {
            var branches = towerPrefab.Progression.Branches;
            TowerLevel basic = towerPrefab.Progression.Base;
            for (int tier = 0; tier < 2; tier++)
            {
                TowerLevel rapid = branches[Rapid].tiers[tier], heavy = branches[Heavy].tiers[tier], balanced = branches[Balanced].tiers[tier];
                Assert.AreEqual(rapid.cost, heavy.cost, "Compared at equal investment.");
                Assert.AreEqual(rapid.cost, balanced.cost, "Compared at equal investment.");

                Assert.Greater(rapid.AttacksPerSecond, 1.4f * balanced.AttacksPerSecond, "Rapid clearly fires fastest.");
                Assert.Greater(rapid.AttacksPerSecond, 3f * heavy.AttacksPerSecond);
                Assert.AreEqual(basic.damage, rapid.damage, "Rapid keeps light hits.");
                Assert.Greater(heavy.damage, 2f * balanced.damage, "Heavy clearly hits hardest.");
                Assert.Less(heavy.AttacksPerSecond, basic.AttacksPerSecond, "Heavy trades attack speed for hit damage.");

                Assert.Greater(balanced.damage, rapid.damage, "Balanced sits between them.");
                Assert.Less(balanced.damage, heavy.damage);
                Assert.Greater(balanced.AttacksPerSecond, heavy.AttacksPerSecond);
                Assert.Less(balanced.AttacksPerSecond, rapid.AttacksPerSecond);
                Assert.Greater(balanced.range, rapid.range, "Balanced's own edge is reach.");
                Assert.Less(balanced.DamagePerSecond, Mathf.Max(rapid.DamagePerSecond, heavy.DamagePerSecond), "Balanced is not simply better.");
            }
        }

        [UnityTest]
        public IEnumerator Rapid_InRealCombat_FiresOftenWithLightShots() => ObserveCombat(Rapid);

        [UnityTest]
        public IEnumerator Heavy_InRealCombat_FiresRarelyWithHeavyShots() => ObserveCombat(Heavy);

        [UnityTest]
        public IEnumerator Balanced_InRealCombat_FiresAtItsOwnRateAndDamage() => ObserveCombat(Balanced);

        /// <summary>Places a tower beside the spawn, commits it to <paramref name="branch"/> and watches it fight wave 1.</summary>
        IEnumerator ObserveCombat(int branch)
        {
            Tower tower = Place(NearSpawnSpot);
            Assert.IsTrue(builder.TryUpgrade(tower, branch));
            TowerLevel tier = tower.Progression.Current;

            Time.timeScale = 2f;
            game.StartNextWave();
            var shotTimes = new List<float>();
            var shotDamage = new List<float>();
            float deadline = Time.time + 20f;
            while (shotTimes.Count < 6 && Time.time < deadline)
            {
                int before = tower.ShotsFired;
                yield return null;
                if (tower.ShotsFired > before)
                {
                    shotTimes.Add(Time.time);
                    foreach (Projectile projectile in Object.FindObjectsByType<Projectile>())
                        shotDamage.Add(projectile.Damage);
                }
            }

            Assert.GreaterOrEqual(shotTimes.Count, 3, "The tower kept firing.");
            float shortestGap = float.MaxValue;
            for (int i = 1; i < shotTimes.Count; i++)
                shortestGap = Mathf.Min(shortestGap, shotTimes[i] - shotTimes[i - 1]);
            TestContext.WriteLine($"{tier.label}: shortest gap between shots {shortestGap:0.00} s (interval {tier.attackInterval}), hits of {tier.damage}");
            Assert.AreEqual(tier.attackInterval, shortestGap, 0.12f, "Shots come at the branch's attack interval.");
            Assert.IsNotEmpty(shotDamage);
            foreach (float damage in shotDamage)
                Assert.AreEqual(tier.damage, damage, "Every shot carries the branch's hit damage.");
        }

        [UnityTest]
        public IEnumerator Selling_RefundsOnce_AndRemovesTheTower()
        {
            game.Session.AddCurrency(100); // test setup
            Tower tower = Place(InsideHorseshoeSpot);
            builder.TryUpgrade(tower, Heavy);
            builder.TryUpgrade(tower, Heavy);
            int invested = 25 + 25 + 45;
            Assert.AreEqual(150 - invested, game.Session.Currency);
            int refund = builder.SellValue(tower);
            Assert.AreEqual(invested * 70 / 100, refund, "70% of everything invested (build and both tiers), rounded down.");

            interaction.Select(tower);
            Assert.IsTrue(builder.TrySell(tower));
            Assert.AreEqual(150 - invested + refund, game.Session.Currency);
            Assert.IsFalse(builder.TrySell(tower), "A tower can only be sold once.");
            Assert.AreEqual(150 - invested + refund, game.Session.Currency);

            yield return null;
            Assert.IsTrue(tower == null, "The sold tower is removed.");
            Assert.IsEmpty(builder.Towers);
            Assert.IsNull(interaction.Selected, "Selling clears the selection.");
            Assert.AreEqual(PlacementResult.Valid, builder.CheckPlacement(towerPrefab, InsideHorseshoeSpot), "Its spot is free again.");
        }

        [UnityTest]
        public IEnumerator SoldSpecialisedTower_StopsActing_WhileEnemiesAndProjectilesCarryOn()
        {
            Tower tower = Place(NearSpawnSpot);
            Assert.IsTrue(builder.TryUpgrade(tower, Rapid));
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
