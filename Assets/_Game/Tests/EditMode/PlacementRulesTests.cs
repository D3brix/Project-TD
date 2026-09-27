using System.Collections.Generic;
using NUnit.Framework;
using ProjectTD.Levels;
using ProjectTD.Placement;
using UnityEngine;

namespace ProjectTD.Tests
{
    public class PlacementRulesTests
    {
        // A 20 x 10 battlefield with a straight road along y = 0 (clearance 0.65) and a pond at (5, 3).
        static readonly Rect Bounds = new Rect(-10f, -5f, 20f, 10f);
        static readonly Vector2[] Road = { new Vector2(-10f, 0f), new Vector2(10f, 0f) };
        static readonly CircleArea Pond = new CircleArea(new Vector2(5f, 3f), 1f);
        const float Footprint = 0.4f;

        static readonly PlacementRules Rules = new PlacementRules(Bounds, Road, 0.65f, new[] { Pond });
        static readonly List<CircleArea> NoTowers = new List<CircleArea>();

        static PlacementResult Check(float x, float y, IEnumerable<CircleArea> towers = null) =>
            Rules.Check(new CircleArea(new Vector2(x, y), Footprint), towers ?? NoTowers);

        [Test]
        public void OpenGrassNextToTheRoad_IsValid()
        {
            Assert.AreEqual(PlacementResult.Valid, Check(0f, 1.1f));
            Assert.AreEqual(PlacementResult.Valid, Check(0f, -3f));
        }

        [Test]
        public void OnOrTooCloseToTheRoad_IsRejected()
        {
            Assert.AreEqual(PlacementResult.OnRoad, Check(0f, 0f));
            Assert.AreEqual(PlacementResult.OnRoad, Check(0f, 0.9f), "The footprint would overlap the road's clearance.");
        }

        [Test]
        public void OutsideOrStraddlingTheBattlefieldEdge_IsRejected()
        {
            Assert.AreEqual(PlacementResult.OutOfBounds, Check(12f, 3f));
            Assert.AreEqual(PlacementResult.OutOfBounds, Check(9.8f, 3f), "The footprint must be fully inside.");
            Assert.AreEqual(PlacementResult.OutOfBounds, Check(0f, -4.8f));
        }

        [Test]
        public void OnBlockedTerrain_IsRejected()
        {
            Assert.AreEqual(PlacementResult.BlockedTerrain, Check(5f, 3f));
            Assert.AreEqual(PlacementResult.BlockedTerrain, Check(6.2f, 3f), "The footprint would overlap the pond's edge.");
            Assert.AreEqual(PlacementResult.Valid, Check(6.5f, 3f));
        }

        [Test]
        public void OverlappingAnotherTower_IsRejected_ButNeighbouringIsFine()
        {
            var towers = new List<CircleArea> { new CircleArea(new Vector2(0f, 2f), Footprint) };

            Assert.AreEqual(PlacementResult.OverlapsTower, Check(0f, 2f, towers));
            Assert.AreEqual(PlacementResult.OverlapsTower, Check(0.7f, 2f, towers));
            Assert.AreEqual(PlacementResult.Valid, Check(0.85f, 2f, towers));
        }
    }
}
