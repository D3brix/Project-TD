using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Enemies
{
    /// <summary>
    /// Moves a point along a polyline of waypoints at whatever distance it is given.
    /// Leftover distance carries around corners, so movement is frame-rate independent.
    /// </summary>
    public class PathFollower
    {
        readonly Vector2[] waypoints;
        int nextWaypoint = 1;

        public Vector2 Position { get; private set; }
        public float DistanceTravelled { get; private set; }
        public bool ReachedEnd { get; private set; }

        public PathFollower(IReadOnlyList<Vector2> waypoints)
        {
            if (waypoints == null || waypoints.Count < 2)
                throw new ArgumentException("A path needs at least two waypoints.", nameof(waypoints));

            this.waypoints = new Vector2[waypoints.Count];
            for (int i = 0; i < waypoints.Count; i++)
                this.waypoints[i] = waypoints[i];

            Position = this.waypoints[0];
        }

        public void Advance(float distance)
        {
            while (distance > 0f && !ReachedEnd)
            {
                Vector2 target = waypoints[nextWaypoint];
                float toTarget = Vector2.Distance(Position, target);

                if (distance < toTarget)
                {
                    Position = Vector2.MoveTowards(Position, target, distance);
                    DistanceTravelled += distance;
                    return;
                }

                Position = target;
                DistanceTravelled += toTarget;
                distance -= toTarget;
                nextWaypoint++;

                if (nextWaypoint >= waypoints.Length)
                    ReachedEnd = true;
            }
        }
    }
}
