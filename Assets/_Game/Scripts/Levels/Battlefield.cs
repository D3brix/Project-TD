using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// The playable area. Towers must be built inside <see cref="Bounds"/>, and the camera frames it.
    /// Terrain features that cannot be built on are <see cref="PlacementBlocker"/>s among this object's children.
    /// </summary>
    public class Battlefield : MonoBehaviour
    {
        [SerializeField] Vector2 size = new Vector2(28f, 13f);

        public Rect Bounds => new Rect((Vector2)transform.position - size / 2f, size);

        public List<CircleArea> GetBlockedAreas()
        {
            var areas = new List<CircleArea>();
            foreach (PlacementBlocker blocker in GetComponentsInChildren<PlacementBlocker>())
                areas.Add(blocker.Area);
            return areas;
        }

        void OnDrawGizmos()
        {
            Rect bounds = Bounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
