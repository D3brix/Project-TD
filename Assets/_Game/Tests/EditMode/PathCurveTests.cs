using NUnit.Framework;
using ProjectTD.Enemies;
using ProjectTD.Levels;
using UnityEngine;

namespace ProjectTD.Tests
{
    public class PathCurveTests
    {
        // A route with two hard 90-degree corners: right, up, right.
        static readonly Vector2[] Corners = { new Vector2(0f, 0f), new Vector2(4f, 0f), new Vector2(4f, 4f), new Vector2(8f, 4f) };

        [Test]
        public void StartsAndEndsExactlyAtTheFirstAndLastControlPoints()
        {
            Vector2[] curve = PathCurve.Sample(Corners, 0.1f);

            Assert.AreEqual(Corners[0], curve[0]);
            Assert.AreEqual(Corners[3], curve[curve.Length - 1]);
        }

        [Test]
        public void PassesThroughEveryControlPoint()
        {
            Vector2[] curve = PathCurve.Sample(Corners, 0.1f);

            foreach (Vector2 control in Corners)
                Assert.Less(PathCurve.DistanceTo(curve, control), 0.02f, $"The curve passes through {control}.");
        }

        [Test]
        public void PointsAreEvenlySpaced()
        {
            Vector2[] curve = PathCurve.Sample(Corners, 0.1f);

            for (int i = 1; i < curve.Length - 1; i++)
                Assert.AreEqual(0.1f, Vector2.Distance(curve[i - 1], curve[i]), 0.01f);
        }

        [Test]
        public void HardCornersBecomeSmoothTurns()
        {
            Vector2[] curve = PathCurve.Sample(Corners, 0.1f);

            Assert.AreEqual(90f, MaxTurnPerSegment(Corners), 1e-3f, "The raw control polygon turns 90 degrees at once.");
            Assert.Less(MaxTurnPerSegment(curve), 15f, "The curve spreads each turn over many samples.");
        }

        [Test]
        public void SameInput_GivesTheSameCurve()
        {
            Assert.AreEqual(PathCurve.Sample(Corners, 0.1f), PathCurve.Sample(Corners, 0.1f));
        }

        [Test]
        public void DistanceTo_MeasuresToTheNearestSegment()
        {
            Vector2[] line = { new Vector2(0f, 0f), new Vector2(10f, 0f) };

            Assert.AreEqual(2f, PathCurve.DistanceTo(line, new Vector2(5f, 2f)), 1e-5f);
            Assert.AreEqual(5f, PathCurve.DistanceTo(line, new Vector2(-3f, 4f)), 1e-5f);
        }

        [Test]
        public void Follower_OnTheCurve_ReachesTheEndExactlyOnce_AtTheCurveLength()
        {
            Vector2[] curve = PathCurve.Sample(Corners, 0.1f);
            float length = PathCurve.Length(curve);
            var follower = new PathFollower(curve);

            float previous = 0f;
            int reachedEndTransitions = 0;
            bool wasAtEnd = false;
            for (int i = 0; i < 500; i++)
            {
                follower.Advance(0.037f);
                Assert.GreaterOrEqual(follower.DistanceTravelled, previous, "Progress never goes backwards.");
                Assert.Less(PathCurve.DistanceTo(curve, follower.Position), 1e-3f, "The follower stays on the curve.");
                previous = follower.DistanceTravelled;
                if (follower.ReachedEnd && !wasAtEnd)
                    reachedEndTransitions++;
                wasAtEnd = follower.ReachedEnd;
            }

            Assert.AreEqual(1, reachedEndTransitions);
            Assert.AreEqual(length, follower.DistanceTravelled, 1e-3f);
            Assert.AreEqual(Corners[3], follower.Position);
        }

        internal static float MaxTurnPerSegment(System.Collections.Generic.IReadOnlyList<Vector2> polyline)
        {
            float max = 0f;
            for (int i = 2; i < polyline.Count; i++)
                max = Mathf.Max(max, Vector2.Angle(polyline[i - 1] - polyline[i - 2], polyline[i] - polyline[i - 1]));
            return max;
        }
    }
}
