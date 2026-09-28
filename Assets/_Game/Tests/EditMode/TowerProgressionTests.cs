using System;
using NUnit.Framework;
using ProjectTD.Towers;

namespace ProjectTD.Tests
{
    public class TowerProgressionTests
    {
        const int Rapid = 0, Heavy = 1, Balanced = 2;

        static TowerProgression NewTower() => new TowerProgression(
            new TowerLevel { label = "Basic", cost = 25, damage = 3f, attackInterval = 0.6f, range = 3f },
            new[]
            {
                Branch("Rapid", (25, 3f, 0.4f, 3f), (45, 3f, 0.26f, 3f)),
                Branch("Heavy", (25, 10f, 1.3f, 3f), (45, 15f, 1.25f, 3f)),
                Branch("Balanced", (20, 3.8f, 0.5f, 3.3f), (40, 4.5f, 0.45f, 3.6f)),
            });

        static TowerBranch Branch(string name, params (int cost, float damage, float interval, float range)[] tiers)
        {
            var branch = new TowerBranch { name = name, tiers = new TowerLevel[tiers.Length] };
            for (int i = 0; i < tiers.Length; i++)
                branch.tiers[i] = new TowerLevel { label = $"{name} {i + 1}", cost = tiers[i].cost, damage = tiers[i].damage, attackInterval = tiers[i].interval, range = tiers[i].range };
            return branch;
        }

        [Test]
        public void StartsAtTheBaseLevel_Uncommitted_WithTheBuildCostInvested_AndEveryBranchOpen()
        {
            var tower = NewTower();

            Assert.IsFalse(tower.HasBranch);
            Assert.AreEqual(0, tower.Tier);
            Assert.AreEqual(3f, tower.Current.damage);
            Assert.AreEqual(25, tower.TotalInvested);
            Assert.IsFalse(tower.IsMaxLevel);
            Assert.AreEqual("Rapid 1", tower.NextIn(Rapid).label);
            Assert.AreEqual("Heavy 1", tower.NextIn(Heavy).label);
            Assert.AreEqual("Balanced 1", tower.NextIn(Balanced).label);
        }

        [TestCase(Rapid)]
        [TestCase(Heavy)]
        [TestCase(Balanced)]
        public void TheFirstUpgrade_CommitsToItsBranch_AndClosesTheOthers(int chosen)
        {
            var tower = NewTower();
            TowerLevel first = tower.NextIn(chosen);

            Assert.IsTrue(tower.Upgrade(chosen));

            Assert.AreEqual(chosen, tower.BranchIndex);
            Assert.AreEqual(1, tower.Tier);
            Assert.AreSame(first, tower.Current);
            Assert.AreEqual(25 + first.cost, tower.TotalInvested);
            for (int other = 0; other < 3; other++)
            {
                if (other == chosen)
                    continue;
                Assert.IsNull(tower.NextIn(other), "Another branch is no longer offered.");
                Assert.IsFalse(tower.Upgrade(other), "Another branch cannot be bought.");
            }
            Assert.AreEqual(chosen, tower.BranchIndex);
            Assert.AreEqual(1, tower.Tier);
            Assert.AreEqual(25 + first.cost, tower.TotalInvested, "Refused upgrades change nothing.");
        }

        [TestCase(Rapid)]
        [TestCase(Heavy)]
        [TestCase(Balanced)]
        public void TheSecondTier_FollowsTheChosenBranch_ThenTheBranchIsComplete(int chosen)
        {
            var tower = NewTower();
            tower.Upgrade(chosen);
            TowerLevel second = tower.NextIn(chosen);
            int invested = tower.TotalInvested;

            Assert.IsTrue(tower.Upgrade(chosen));
            Assert.AreSame(second, tower.Current);
            Assert.AreEqual(2, tower.Tier);
            Assert.AreEqual(invested + second.cost, tower.TotalInvested);

            Assert.IsTrue(tower.IsMaxLevel);
            Assert.IsNull(tower.NextIn(chosen));
            Assert.IsFalse(tower.Upgrade(chosen), "The last tier cannot be bought again.");
            Assert.AreEqual(invested + second.cost, tower.TotalInvested);
        }

        [Test]
        public void UnknownBranches_AreRefused()
        {
            var tower = NewTower();

            Assert.IsNull(tower.NextIn(-1));
            Assert.IsNull(tower.NextIn(3));
            Assert.IsFalse(tower.Upgrade(3));
            Assert.IsFalse(tower.HasBranch);
        }

        [Test]
        public void SellValue_IsAFixedPercentageOfEverythingInvested_RoundedDown()
        {
            var tower = NewTower();
            Assert.AreEqual(17, tower.SellValue(70));

            tower.Upgrade(Heavy);
            tower.Upgrade(Heavy);
            Assert.AreEqual((25 + 25 + 45) * 70 / 100, tower.SellValue(70));
            Assert.AreEqual(95, tower.SellValue(100));
            Assert.AreEqual(0, tower.SellValue(0));
        }

        [Test]
        public void ABranchWithoutTiers_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => new TowerProgression(new TowerLevel(), new[] { new TowerBranch { tiers = new TowerLevel[0] } }));
        }
    }
}
