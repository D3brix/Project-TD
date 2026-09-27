using System.Collections.Generic;
using NUnit.Framework;
using ProjectTD.Enemies;
using ProjectTD.Towers;
using UnityEngine;

namespace ProjectTD.Tests
{
    public class TargetSelectorTests
    {
        // A straight path along the x axis from -10 to 10.
        static readonly Vector2[] StraightPath = { new Vector2(-10f, 0f), new Vector2(10f, 0f) };

        readonly List<GameObject> created = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in created)
                Object.DestroyImmediate(go);
            created.Clear();
        }

        Enemy CreateEnemyAt(float distanceAlongPath)
        {
            var go = new GameObject("TestEnemy");
            created.Add(go);
            Enemy enemy = go.AddComponent<Enemy>();
            enemy.Initialize(StraightPath);
            enemy.Move(distanceAlongPath);
            return enemy;
        }

        [Test]
        public void First_PicksEnemyFurthestAlongPath_WithinRange()
        {
            Enemy behind = CreateEnemyAt(9f);    // x = -1
            Enemy ahead = CreateEnemyAt(11f);    // x = 1
            Enemy middle = CreateEnemyAt(10f);   // x = 0

            Enemy target = TargetSelector.Select(new[] { behind, ahead, middle }, Vector2.zero, 3f, TargetingMode.First);

            Assert.AreSame(ahead, target);
        }

        [Test]
        public void IgnoresEnemiesOutOfRange_EvenIfFurtherAlong()
        {
            Enemy inRange = CreateEnemyAt(10f);   // x = 0
            Enemy outOfRange = CreateEnemyAt(15f); // x = 5

            Enemy target = TargetSelector.Select(new[] { inRange, outOfRange }, Vector2.zero, 3f, TargetingMode.First);

            Assert.AreSame(inRange, target);
        }

        [Test]
        public void ReturnsNull_WhenNoEnemyInRange()
        {
            Enemy far = CreateEnemyAt(0f); // x = -10

            Assert.IsNull(TargetSelector.Select(new[] { far }, Vector2.zero, 3f, TargetingMode.First));
            Assert.IsNull(TargetSelector.Select(new Enemy[0], Vector2.zero, 3f, TargetingMode.First));
        }

        [Test]
        public void Ties_KeepTheEarlierEnemy()
        {
            Enemy first = CreateEnemyAt(10f);
            Enemy second = CreateEnemyAt(10f);

            Assert.AreSame(first, TargetSelector.Select(new[] { first, second }, Vector2.zero, 3f, TargetingMode.First));
        }

        [Test]
        public void IgnoresUninitializedEnemies()
        {
            var go = new GameObject("Uninitialized");
            created.Add(go);
            Enemy uninitialized = go.AddComponent<Enemy>();

            Assert.IsNull(TargetSelector.Select(new[] { uninitialized }, Vector2.zero, 3f, TargetingMode.First));
        }
    }
}
