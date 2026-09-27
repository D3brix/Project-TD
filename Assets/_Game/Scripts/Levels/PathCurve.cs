using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// Turns a handful of authored control points into a smooth route: a centripetal Catmull-Rom spline
    /// through every control point, resampled into a dense polyline with (almost) evenly spaced points.
    /// Enemies walk that polyline, so "distance along the path" stays a plain, exact number.
    /// </summary>
    public static class PathCurve
    {
        const int SubdivisionsPerSpacing = 4;

        public static Vector2[] Sample(IReadOnlyList<Vector2> controlPoints, float spacing)
        {
            if (controlPoints == null || controlPoints.Count < 2)
                throw new ArgumentException("A path needs at least two control points.", nameof(controlPoints));
            if (spacing <= 0f)
                throw new ArgumentOutOfRangeException(nameof(spacing), "Spacing must be positive.");

            int n = controlPoints.Count;
            var dense = new List<Vector2>();
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p1 = controlPoints[i];
                Vector2 p2 = controlPoints[i + 1];
                // Mirror the neighbours at both ends so the curve starts and ends heading along the first/last segment.
                Vector2 p0 = i > 0 ? controlPoints[i - 1] : 2f * p1 - p2;
                Vector2 p3 = i + 2 < n ? controlPoints[i + 2] : 2f * p2 - p1;

                int steps = Mathf.Max(8, Mathf.CeilToInt(Vector2.Distance(p1, p2) / spacing * SubdivisionsPerSpacing));
                for (int s = 0; s < steps; s++)
                    dense.Add(CentripetalCatmullRom(p0, p1, p2, p3, s / (float)steps));
            }
            dense.Add(controlPoints[n - 1]);

            return Resample(dense, spacing);
        }

        public static float Length(IReadOnlyList<Vector2> polyline)
        {
            float length = 0f;
            for (int i = 1; i < polyline.Count; i++)
                length += Vector2.Distance(polyline[i - 1], polyline[i]);
            return length;
        }

        /// <summary>Shortest distance from <paramref name="point"/> to the polyline.</summary>
        public static float DistanceTo(IReadOnlyList<Vector2> polyline, Vector2 point)
        {
            float bestSqr = float.MaxValue;
            for (int i = 1; i < polyline.Count; i++)
            {
                Vector2 a = polyline[i - 1];
                Vector2 ab = polyline[i] - a;
                float lengthSqr = ab.sqrMagnitude;
                float t = lengthSqr > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / lengthSqr) : 0f;
                bestSqr = Mathf.Min(bestSqr, (a + ab * t - point).sqrMagnitude);
            }
            return Mathf.Sqrt(bestSqr);
        }

        // Barry-Goldman form of the centripetal Catmull-Rom spline between p1 and p2 (u in [0, 1]).
        // Centripetal parameterisation never forms cusps or self-intersecting loops between control points.
        static Vector2 CentripetalCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float u)
        {
            float t0 = 0f;
            float t1 = t0 + Knot(p0, p1);
            float t2 = t1 + Knot(p1, p2);
            float t3 = t2 + Knot(p2, p3);
            float t = Mathf.Lerp(t1, t2, u);

            Vector2 a1 = Blend(p0, p1, t0, t1, t);
            Vector2 a2 = Blend(p1, p2, t1, t2, t);
            Vector2 a3 = Blend(p2, p3, t2, t3, t);
            Vector2 b1 = Blend(a1, a2, t0, t2, t);
            Vector2 b2 = Blend(a2, a3, t1, t3, t);
            return Blend(b1, b2, t1, t2, t);
        }

        static float Knot(Vector2 a, Vector2 b) => Mathf.Max(1e-4f, Mathf.Sqrt(Vector2.Distance(a, b)));

        static Vector2 Blend(Vector2 a, Vector2 b, float ta, float tb, float t) => (tb - t) / (tb - ta) * a + (t - ta) / (tb - ta) * b;

        static Vector2[] Resample(List<Vector2> dense, float spacing)
        {
            var result = new List<Vector2> { dense[0] };
            float carried = 0f;
            for (int i = 1; i < dense.Count; i++)
            {
                Vector2 a = dense[i - 1];
                Vector2 b = dense[i];
                float segment = Vector2.Distance(a, b);
                float along = spacing - carried;
                while (along <= segment)
                {
                    result.Add(Vector2.Lerp(a, b, along / segment));
                    along += spacing;
                }
                carried = segment - (along - spacing);
            }

            Vector2 end = dense[dense.Count - 1];
            if (Vector2.Distance(result[result.Count - 1], end) > spacing * 0.25f)
                result.Add(end);
            else
                result[result.Count - 1] = end;
            return result.ToArray();
        }
    }
}
