using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// Keeps the whole battlefield in view between the HUD's top and bottom bars, at any screen aspect ratio.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class BattlefieldCamera : MonoBehaviour
    {
        [SerializeField] Battlefield battlefield;
        [Tooltip("Extra world-space border around the battlefield.")]
        [SerializeField, Min(0f)] float margin = 0.25f;
        [Tooltip("Fraction of the screen height covered by the HUD's top bar.")]
        [SerializeField, Range(0f, 0.4f)] float topHud = 0.065f;
        [Tooltip("Fraction of the screen height covered by the HUD's bottom bar.")]
        [SerializeField, Range(0f, 0.4f)] float bottomHud = 0.11f;

        Camera cam;

        void LateUpdate()
        {
            Fit();
        }

        public void Fit()
        {
            if (battlefield == null)
                return;
            if (cam == null)
                cam = GetComponent<Camera>();

            Rect area = battlefield.Bounds;
            area.min -= Vector2.one * margin;
            area.max += Vector2.one * margin;

            float size = RequiredOrthographicSize(area.size, cam.aspect, topHud, bottomHud);
            cam.orthographicSize = size;
            Vector3 position = transform.position;
            transform.position = new Vector3(area.center.x, CameraY(area.center.y, size, topHud, bottomHud), position.z);
        }

        /// <summary>The smallest orthographic size that fits <paramref name="areaSize"/> into the screen band between the HUD bars.</summary>
        public static float RequiredOrthographicSize(Vector2 areaSize, float aspect, float topHud, float bottomHud)
        {
            float usableFraction = Mathf.Max(0.1f, 1f - topHud - bottomHud);
            float heightForArea = areaSize.y / usableFraction;
            float heightForWidth = areaSize.x / aspect;
            return Mathf.Max(heightForArea, heightForWidth) / 2f;
        }

        /// <summary>Camera y that centres the area in the band between the HUD bars.</summary>
        public static float CameraY(float areaCenterY, float orthographicSize, float topHud, float bottomHud)
        {
            return areaCenterY - (bottomHud - topHud) * orthographicSize;
        }
    }
}
