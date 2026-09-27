using NUnit.Framework;
using ProjectTD.Towers;

namespace ProjectTD.Tests
{
    public class TowerProgressionTests
    {
        static TowerLevel[] Levels() => new[]
        {
            new TowerLevel { label = "Basic", cost = 30, damage = 3f, attackInterval = 0.6f, range = 3f },
            new TowerLevel { label = "Heavy", cost = 20, damage = 4.5f, attackInterval = 0.6f, range = 3f },
            new TowerLevel { label = "Fast", cost = 35, damage = 4.5f, attackInterval = 0.45f, range = 3f },
        };

        [Test]
        public void StartsAtBaseLevel_WithTheBuildCostInvested()
        {
            var tower = new TowerProgression(Levels());

            Assert.AreEqual(0, tower.LevelIndex);
            Assert.AreEqual(3f, tower.Current.damage);
            Assert.AreEqual(30, tower.TotalInvested);
            Assert.AreEqual(20, tower.Next.cost);
        }

        [Test]
        public void Upgrade_AppliesTheNextLevel_AndAddsItsCost()
        {
            var tower = new TowerProgression(Levels());

            Assert.IsTrue(tower.Upgrade());
            Assert.AreEqual(4.5f, tower.Current.damage);
            Assert.AreEqual(50, tower.TotalInvested);

            Assert.IsTrue(tower.Upgrade());
            Assert.AreEqual(0.45f, tower.Current.attackInterval);
            Assert.AreEqual(85, tower.TotalInvested);
        }

        [Test]
        public void AtMaxLevel_UpgradeIsRefused_AndNothingChanges()
        {
            var tower = new TowerProgression(Levels());
            tower.Upgrade();
            tower.Upgrade();

            Assert.IsTrue(tower.IsMaxLevel);
            Assert.IsNull(tower.Next);
            Assert.IsFalse(tower.Upgrade());
            Assert.AreEqual(2, tower.LevelIndex);
            Assert.AreEqual(85, tower.TotalInvested);
        }

        [Test]
        public void SellValue_IsAFixedPercentageOfEverythingInvested_RoundedDown()
        {
            var tower = new TowerProgression(Levels());
            Assert.AreEqual(21, tower.SellValue(70));

            tower.Upgrade();
            Assert.AreEqual(35, tower.SellValue(70));
            Assert.AreEqual(50, tower.SellValue(100));
            Assert.AreEqual(0, tower.SellValue(0));
        }
    }
}
