using System.Collections.Generic;
using UnityEngine;

namespace ProjectTD.Levels
{
    /// <summary>
    /// Draws the dirt road along the exact curve enemies walk (<see cref="LevelPath.Points"/>), so what the
    /// player sees is where enemies go. Placeholder art: vertex-coloured meshes with a soft worn verge, irregular
    /// edges, a lighter worn centre and faint cart ruts. Irregularity is deterministic noise, not randomness.
    /// </summary>
    [ExecuteAlways]
    public class RoadRenderer : MonoBehaviour
    {
        [SerializeField] LevelPath path;
        [SerializeField] MeshFilter vergeLayer;
        [SerializeField] MeshFilter dirtLayer;
        [SerializeField] MeshFilter detailLayer;
        [Tooltip("How far the road visibly continues past the spawn point and the end point.")]
        [SerializeField, Min(0f)] float extendStart = 3f;
        [SerializeField, Min(0f)] float extendEnd = 1f;
        [SerializeField, Range(0f, 0.5f)] float widthVariation = 0.22f;
        [SerializeField] float noiseSeed = 17.3f;
        [SerializeField] Color vergeColor = new Color(0.52f, 0.55f, 0.32f, 0.6f);
        [SerializeField] Color dirtColor = new Color(0.6f, 0.47f, 0.32f);
        [SerializeField] Color wornColor = new Color(0.68f, 0.56f, 0.4f);
        [SerializeField] Color rutColor = new Color(0.47f, 0.36f, 0.25f, 0.8f);

        Vector2[] builtFrom;

        void OnEnable()
        {
            Rebuild();
        }

        void OnDisable()
        {
            builtFrom = null;
        }

#if UNITY_EDITOR
        void Update()
        {
            // In the editor, follow control-point edits so the road can be shaped by dragging them.
            if (!Application.isPlaying && path != null && !SameControlPoints(path.GetControlPoints()))
            {
                path.Rebuild();
                Rebuild();
            }
        }
#endif

        public void Rebuild()
        {
            if (path == null || vergeLayer == null || dirtLayer == null || detailLayer == null || path.transform.childCount < 2)
                return;

            builtFrom = path.GetControlPoints();
            List<Vector2> points = Extend(path.Points);
            int count = points.Count;

            var normals = new Vector2[count];
            var distance = new float[count];
            var bend = new float[count]; // signed curvature, positive when turning left
            for (int i = 0; i < count; i++)
            {
                Vector2 tangent = (points[Mathf.Min(i + 1, count - 1)] - points[Mathf.Max(i - 1, 0)]).normalized;
                normals[i] = new Vector2(-tangent.y, tangent.x);
                distance[i] = i == 0 ? -extendStart : distance[i - 1] + Vector2.Distance(points[i - 1], points[i]);
            }
            for (int i = 1; i < count - 1; i++)
            {
                Vector2 a = (points[i] - points[i - 1]).normalized;
                Vector2 b = (points[i + 1] - points[i]).normalized;
                float step = Mathf.Max(1e-4f, 0.5f * (Vector2.Distance(points[i - 1], points[i]) + Vector2.Distance(points[i], points[i + 1])));
                bend[i] = Vector2.SignedAngle(a, b) * Mathf.Deg2Rad / step;
            }
            Smooth(bend, 6);

            float half = path.RoadHalfWidth;
            var dirtLeft = new float[count];
            var dirtRight = new float[count];
            var vergeLeft = new float[count];
            var vergeRight = new float[count];
            var wornLeft = new float[count];
            var wornRight = new float[count];
            var rutA = new float[count];
            var rutB = new float[count];
            for (int i = 0; i < count; i++)
            {
                float s = distance[i];
                // Carts cut the inside of bends and swing wide on the outside, so bends are a little wider.
                float widen = Mathf.Min(0.12f, Mathf.Abs(bend[i]) * 0.18f);
                float inside = Mathf.Clamp(bend[i] * 0.12f, -0.08f, 0.08f);

                // Each edge wanders on its own: a slow drift plus a little fine raggedness.
                dirtLeft[i] = half * (1f + widthVariation * (0.75f * Noise(s, 0.31f, 1.7f) + 0.25f * Noise(s, 1.9f, 2.9f))) + widen * (bend[i] > 0f ? 0.4f : 1f);
                dirtRight[i] = half * (1f + widthVariation * (0.75f * Noise(s, 0.29f, 5.3f) + 0.25f * Noise(s, 2.1f, 6.1f))) + widen * (bend[i] < 0f ? 0.4f : 1f);
                vergeLeft[i] = dirtLeft[i] + 0.16f + 0.1f * Noise(s, 0.7f, 9.1f) + 0.03f * Noise(s, 2.6f, 10.2f);
                vergeRight[i] = dirtRight[i] + 0.16f + 0.1f * Noise(s, 0.65f, 12.4f) + 0.03f * Noise(s, 2.4f, 13.5f);
                wornLeft[i] = 0.2f + 0.06f * Noise(s, 0.5f, 3.3f) + inside;
                wornRight[i] = 0.2f + 0.06f * Noise(s, 0.55f, 7.7f) - inside;
                rutA[i] = 0.27f + 0.035f * Noise(s, 0.7f, 21.1f) + inside;
                rutB[i] = -0.27f - 0.035f * Noise(s, 0.65f, 25.9f) + inside;
            }

            var verge = new MeshBuilder();
            verge.AddBand(points, normals, i => dirtLeft[i] - 0.06f, i => vergeLeft[i], i => Shade(vergeColor, distance[i], 1f), i => Shade(vergeColor, distance[i], 0f));
            verge.AddBand(points, normals, i => -dirtRight[i] + 0.06f, i => -vergeRight[i], i => Shade(vergeColor, distance[i], 1f), i => Shade(vergeColor, distance[i], 0f));

            var dirt = new MeshBuilder();
            dirt.AddBand(points, normals, i => dirtLeft[i], i => -dirtRight[i], i => Shade(dirtColor, distance[i], 1f), i => Shade(dirtColor, distance[i], 1f));
            // Soft fringe so the dirt fades into the verge instead of ending in a hard line.
            dirt.AddBand(points, normals, i => dirtLeft[i], i => dirtLeft[i] + 0.08f, i => Shade(dirtColor, distance[i], 1f), i => Shade(dirtColor, distance[i], 0f));
            dirt.AddBand(points, normals, i => -dirtRight[i], i => -dirtRight[i] - 0.08f, i => Shade(dirtColor, distance[i], 1f), i => Shade(dirtColor, distance[i], 0f));

            var detail = new MeshBuilder();
            detail.AddBand(points, normals, i => wornLeft[i], i => -wornRight[i], i => Shade(wornColor, distance[i], 0.55f), i => Shade(wornColor, distance[i], 0.55f));
            // Ruts come and go: deep in some stretches, worn away in others.
            detail.AddBand(points, normals, i => rutA[i] + 0.035f, i => rutA[i] - 0.035f, i => Shade(rutColor, distance[i], RutStrength(distance[i], 41.3f)), i => Shade(rutColor, distance[i], RutStrength(distance[i], 41.3f)));
            detail.AddBand(points, normals, i => rutB[i] + 0.035f, i => rutB[i] - 0.035f, i => Shade(rutColor, distance[i], RutStrength(distance[i], 47.9f)), i => Shade(rutColor, distance[i], RutStrength(distance[i], 47.9f)));

            Assign(vergeLayer, verge.Build("Road Verge"));
            Assign(dirtLayer, dirt.Build("Road Dirt"));
            Assign(detailLayer, detail.Build("Road Detail"));
        }

        List<Vector2> Extend(IReadOnlyList<Vector2> curve)
        {
            const float step = 0.1f;
            var points = new List<Vector2>(curve.Count + 64);
            Vector2 startDirection = (curve[0] - curve[Mathf.Min(3, curve.Count - 1)]).normalized;
            for (float d = extendStart; d > 0.001f; d -= step)
                points.Add(curve[0] + startDirection * d);
            points.AddRange(curve);
            Vector2 endDirection = (curve[curve.Count - 1] - curve[Mathf.Max(0, curve.Count - 4)]).normalized;
            for (float d = step; d <= extendEnd + 0.001f; d += step)
                points.Add(curve[curve.Count - 1] + endDirection * d);
            return points;
        }

        // Smooth, deterministic value in roughly [-1, 1] that varies along the road.
        float Noise(float distanceAlong, float frequency, float channel)
        {
            return (Mathf.PerlinNoise(distanceAlong * frequency + noiseSeed, channel * 3.1f + noiseSeed) - 0.5f) * 2f;
        }

        float RutStrength(float distanceAlong, float channel) => Mathf.Clamp01(0.3f + 1.4f * Noise(distanceAlong, 0.22f, channel));

        Color Shade(Color baseColor, float distanceAlong, float alphaScale)
        {
            float light = 1f + 0.07f * Noise(distanceAlong, 0.45f, 31.7f);
            var color = new Color(baseColor.r * light, baseColor.g * light, baseColor.b * light, baseColor.a * alphaScale);
            // Vertex colours are used as-is by the shader, so convert them like sprite colours are in linear space.
            return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
        }

        static void Smooth(float[] values, int passes)
        {
            var copy = new float[values.Length];
            for (int p = 0; p < passes; p++)
            {
                values.CopyTo(copy, 0);
                for (int i = 1; i < values.Length - 1; i++)
                    values[i] = (copy[i - 1] + copy[i] + copy[i + 1]) / 3f;
            }
        }

        static void Assign(MeshFilter filter, Mesh mesh)
        {
            Mesh old = filter.sharedMesh;
            filter.sharedMesh = mesh;
            if (old != null && (old.hideFlags & HideFlags.DontSave) != 0)
            {
                if (Application.isPlaying)
                    Destroy(old);
                else
                    DestroyImmediate(old);
            }
        }

        bool SameControlPoints(Vector2[] controlPoints)
        {
            if (builtFrom == null || builtFrom.Length != controlPoints.Length)
                return false;
            for (int i = 0; i < controlPoints.Length; i++)
                if (builtFrom[i] != controlPoints[i])
                    return false;
            return true;
        }

        /// <summary>Collects strips ("bands") between two offsets from the centre line into one mesh.</summary>
        class MeshBuilder
        {
            readonly List<Vector3> vertices = new List<Vector3>();
            readonly List<Color> colors = new List<Color>();
            readonly List<int> triangles = new List<int>();

            public void AddBand(List<Vector2> points, Vector2[] normals, System.Func<int, float> offsetA, System.Func<int, float> offsetB,
                System.Func<int, Color> colorA, System.Func<int, Color> colorB)
            {
                int first = vertices.Count;
                for (int i = 0; i < points.Count; i++)
                {
                    vertices.Add(points[i] + normals[i] * offsetA(i));
                    vertices.Add(points[i] + normals[i] * offsetB(i));
                    colors.Add(colorA(i));
                    colors.Add(colorB(i));

                    if (i == 0)
                        continue;
                    int a = first + (i - 1) * 2;
                    int b = first + i * 2;
                    triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
                }
            }

            public Mesh Build(string name)
            {
                var mesh = new Mesh { name = name, hideFlags = HideFlags.DontSave };
                mesh.SetVertices(vertices);
                mesh.SetColors(colors);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
