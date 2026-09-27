using NUnit.Framework;
using ProjectTD.Enemies;
using UnityEngine;

namespace ProjectTD.Tests
{
    public class PathFollowerTests
    {
        // An L-shaped path: 4 units right, then 3 units up. Total length 7.
        static readonly Vector2[] LPath = { new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 3f) };

        [Test]
        public void StartsAtFirstWaypoint()
        {
            var follower = new PathFollower(LPath);

            Assert.AreEqual(LPath[0], follower.Position);
            Assert.AreEqual(0f, follower.DistanceTravelled);
            Assert.IsFalse(follower.ReachedEnd);
        }

        [Test]
        public void Advance_MovesTowardNextWaypoint()
        {
            var follower = new PathFollower(LPath);

            follower.Advance(1.5f);

            Assert.AreEqual(new Vector2(1.5f, 0f), follower.Position);
            Assert.AreEqual(1.5f, follower.DistanceTravelled, 1e-5f);
        }

        [Test]
        public void Advance_CarriesLeftoverDistanceAroundCorners()
        {
            var follower = new PathFollower(LPath);

            follower.Advance(5f);

            Assert.That(Vector2.Distance(new Vector2(4f, 1f), follower.Position), Is.LessThan(1e-5f));
            Assert.AreEqual(5f, follower.DistanceTravelled, 1e-5f);
        }

        [Test]
        public void ManySmallSteps_MatchOneLargeStep()
        {
            var small = new PathFollower(LPath);
            var large = new PathFollower(LPath);

            for (int i = 0; i < 60; i++)
                small.Advance(0.1f);
            large.Advance(6f);

            Assert.That(Vector2.Distance(small.Position, large.Position), Is.LessThan(1e-4f));
        }

        [Test]
        public void ReachesEnd_AndStopsThere()
        {
            var follower = new PathFollower(LPath);

            follower.Advance(100f);

            Assert.IsTrue(follower.ReachedEnd);
            Assert.AreEqual(LPath[2], follower.Position);
            Assert.AreEqual(7f, follower.DistanceTravelled, 1e-5f);

            follower.Advance(5f);
            Assert.AreEqual(LPath[2], follower.Position);
            Assert.AreEqual(7f, follower.DistanceTravelled, 1e-5f);
        }

        [Test]
        public void RequiresAtLeastTwoWaypoints()
        {
            Assert.Throws<System.ArgumentException>(() => new PathFollower(new[] { Vector2.zero }));
        }
    }
}
