using System.Collections.Generic;
using ProjectTD.Levels;
using UnityEngine;

namespace ProjectTD.Placement
{
    public enum PlacementResult
    {
        Valid,
        OutOfBounds,
        OnRoad,
        BlockedTerrain,
        OverlapsTower,
        NotEnoughCurrency,
        GameOver
    }

    /// <summary>
    /// Where a tower footprint (a circle) may stand on this level: fully inside the battlefield,
    /// clear of the road, clear of blocked terrain and clear of other towers. Pure geometry, no Unity lifecycle.
    /// </summary>
    public class PlacementRules
    {
        readonly Rect bounds;
        readonly IReadOnlyList<Vector2> road;
        readonly float roadClearance;
        readonly IReadOnlyList<CircleArea> blocked;

        public PlacementRules(Rect bounds, IReadOnlyList<Vector2> road, float roadClearance, IReadOnlyList<CircleArea> blocked)
        {
            this.bounds = bounds;
            this.road = road;
            this.roadClearance = roadClearance;
            this.blocked = blocked ?? new CircleArea[0];
        }

        public PlacementResult Check(CircleArea footprint, IEnumerable<CircleArea> towers)
        {
            Vector2 c = footprint.Center;
            float r = footprint.Radius;
            if (c.x - r < bounds.xMin || c.x + r > bounds.xMax || c.y - r < bounds.yMin || c.y + r > bounds.yMax)
                return PlacementResult.OutOfBounds;

            if (PathCurve.DistanceTo(road, c) < roadClearance + r)
                return PlacementResult.OnRoad;

            foreach (CircleArea area in blocked)
                if (footprint.Overlaps(area))
                    return PlacementResult.BlockedTerrain;

            foreach (CircleArea tower in towers)
                if (footprint.Overlaps(tower))
                    return PlacementResult.OverlapsTower;

            return PlacementResult.Valid;
        }

        public static string Describe(PlacementResult result)
        {
            switch (result)
            {
                case PlacementResult.Valid: return "Click to build here";
                case PlacementResult.OutOfBounds: return "Outside the battlefield";
                case PlacementResult.OnRoad: return "Too close to the road";
                case PlacementResult.BlockedTerrain: return "Can't build on this terrain";
                case PlacementResult.OverlapsTower: return "Too close to another tower";
                case PlacementResult.NotEnoughCurrency: return "Not enough gold";
                case PlacementResult.GameOver: return "The game is over";
                default: return result.ToString();
            }
        }
    }
}
