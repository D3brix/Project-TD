using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// The single authored route enemies walk. The control points are this object's children, in hierarchy
    /// order (first child = spawn, last child = end). They are joined by a smooth curve (see <see cref="PathCurve"/>);
    /// enemies, the visible road and tower placement all use that same curve.
    /// </summary>
    public class LevelPath : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] float sampleSpacing = 0.1f;
        [Tooltip("Nominal half-width of the visible road.")]
        [SerializeField, Min(0.1f)] float roadHalfWidth = 0.5f;
        [Tooltip("Nothing can be built closer than this to the road's centre line (before adding the tower's own footprint).")]
        [SerializeField, Min(0.1f)] float buildClearance = 0.65f;

        Vector2[] points;
        float length;

        /// <summary>The curve as a dense polyline, from spawn to end.</summary>
        public IReadOnlyList<Vector2> Points
        {
            get
            {
                if (points == null)
                    Rebuild();
                return points;
            }
        }

        public float Length
        {
            get
            {
                if (points == null)
                    Rebuild();
                return length;
            }
        }

        public float RoadHalfWidth => roadHalfWidth;
        public float BuildClearance => buildClearance;
        public Vector2 Start => Points[0];
        public Vector2 End => Points[Points.Count - 1];

        public Vector2[] GetControlPoints()
        {
            var controlPoints = new Vector2[transform.childCount];
            for (int i = 0; i < controlPoints.Length; i++)
                controlPoints[i] = transform.GetChild(i).position;
            return controlPoints;
        }

        public float DistanceTo(Vector2 point) => PathCurve.DistanceTo(Points, point);

        public void Rebuild()
        {
            points = PathCurve.Sample(GetControlPoints(), sampleSpacing);
            length = PathCurve.Length(points);
        }

        void OnValidate()
        {
            points = null;
        }

        void OnDrawGizmos()
        {
            if (transform.childCount < 2)
                return;

            Vector2[] curve = PathCurve.Sample(GetControlPoints(), sampleSpacing);
            Gizmos.color = Color.yellow;
            for (int i = 1; i < curve.Length; i++)
                Gizmos.DrawLine(curve[i - 1], curve[i]);

            Gizmos.color = new Color(1f, 0.6f, 0f);
            for (int i = 0; i < transform.childCount; i++)
                Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.15f);
        }
    }
}
