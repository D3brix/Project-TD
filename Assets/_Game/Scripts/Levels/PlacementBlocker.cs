using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>Marks a circle of terrain (water, rocks, trees, ruins) where towers cannot be built.</summary>
    public class PlacementBlocker : MonoBehaviour
    {
        [SerializeField, Min(0.05f)] float radius = 0.5f;

        public CircleArea Area => new CircleArea(transform.position, radius);

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    public readonly struct CircleArea
    {
        public readonly Vector2 Center;
        public readonly float Radius;

        public CircleArea(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Overlaps(CircleArea other) => (Center - other.Center).sqrMagnitude < (Radius + other.Radius) * (Radius + other.Radius);
    }
}
