using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// The route enemies walk. The waypoints are this object's children, in hierarchy order:
    /// the first child is the spawn point and the last child is the end point.
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        public Vector2[] GetPoints()
        {
            var points = new Vector2[transform.childCount];
            for (int i = 0; i < points.Length; i++)
                points[i] = transform.GetChild(i).position;
            return points;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int i = 1; i < transform.childCount; i++)
                Gizmos.DrawLine(transform.GetChild(i - 1).position, transform.GetChild(i).position);
        }
    }
}
