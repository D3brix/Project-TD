using System.Collections.Generic;
using ProjectTD.Enemies;
using UnityEngine;

namespace ProjectTD.Towers
{
    public enum TargetingMode
    {
        /// <summary>The enemy furthest along the path (closest to the end).</summary>
        First
    }

    /// <summary>
    /// Picks one enemy in range according to a targeting mode. To add a mode, add an enum value
    /// and a case in <see cref="IsBetter"/>. Ties keep the earlier enemy in the list (spawn order).
    /// </summary>
    public static class TargetSelector
    {
        public static Enemy Select(IReadOnlyList<Enemy> enemies, Vector2 origin, float range, TargetingMode mode)
        {
            Enemy best = null;
            float rangeSqr = range * range;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy candidate = enemies[i];
                if (candidate == null || !candidate.IsAlive)
                    continue;
                if ((candidate.Position - origin).sqrMagnitude > rangeSqr)
                    continue;

                if (best == null || IsBetter(mode, candidate, best))
                    best = candidate;
            }

            return best;
        }

        static bool IsBetter(TargetingMode mode, Enemy candidate, Enemy best)
        {
            switch (mode)
            {
                case TargetingMode.First:
                default:
                    return candidate.PathProgress > best.PathProgress;
            }
        }
    }
}
