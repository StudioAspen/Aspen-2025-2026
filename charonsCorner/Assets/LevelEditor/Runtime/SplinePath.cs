using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using NaughtyAttributes;

namespace CharonsCorner.LevelEditor
{
#if UNITY_EDITOR
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode()]
#endif
    public class SplinePath : MonoBehaviour
    {
#if UNITY_EDITOR
        [Header("References")]
        [SerializeField] private SplineContainer splineContainer;
        [SerializeField] private MeshFilter meshFilter;

        [Header("Config")]
        [SerializeField, Min(0.01f)] private float width = 10f;
        [SerializeField, Min(0.01f)] private float thickness = 1f; // how tall the mesh is
        [SerializeField, Min(1)] private int segments = 50;

        private List<Vector3> leftVertices = new();
        private List<Vector3> rightVertices = new();

        [Header("Debug Config")]
        [SerializeField] private bool enableGizmos;
        [SerializeField, ShowIf("enableGizmos"), Range(0f, 1f)] private float gizmoRadius = 0.2f;

        private void OnEnable()
        {
            Spline.Changed += Spline_Changed;
        }

        private void OnDisable()
        {
            Spline.Changed -= Spline_Changed;
        }

        private void OnValidate()
        {
            if(splineContainer == null)
                splineContainer = GetComponent<SplineContainer>();

            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            RebuildMesh();
            RebuildMeshCollider();
        }

        private void OnDrawGizmos()
        {
            if (!enableGizmos)
                return;

            if(gizmoRadius > 0f)
            {
                for (int i = 0; i < leftVertices.Count; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(transform.TransformPoint(leftVertices[i]), gizmoRadius);
                    Gizmos.DrawSphere(transform.TransformPoint(rightVertices[i]), gizmoRadius);
                }
            }
        }

        private void Spline_Changed(Spline spline, int arg2, SplineModification modification)
        {
            RebuildMesh();
            RebuildMeshCollider();
        }

        private void Update()
        {
            RebuildMesh();
        }

        private void RebuildMesh()
        {
            GetVertices();
            BuildMesh();
        }

        private void GetVertices()
        {
            leftVertices = new();
            rightVertices = new();

            float step = 1f / segments;

            for (int currentSplineIndex = 0; currentSplineIndex < splineContainer.Splines.Count; currentSplineIndex++)
            {
                for (int currentSegment = 0; currentSegment < segments; currentSegment++)
                {
                    float parameter = currentSegment * step;
                    SampleSplineWidth(currentSplineIndex, parameter, width, out Vector3 leftPoint, out Vector3 rightPoint);
                    leftVertices.Add(leftPoint);
                    rightVertices.Add(rightPoint);
                }

                SampleSplineWidth(currentSplineIndex, 1f, width, out Vector3 lastLeftPoint, out Vector3 lastRightPoint);
                leftVertices.Add(lastLeftPoint);
                rightVertices.Add(lastRightPoint);
            }
        }

        private void SampleSplineWidth(int splineIndex, float parameter, float width, out Vector3 leftPoint, out Vector3 rightPoint)
        {
            splineContainer.Evaluate(splineIndex, parameter, out float3 position, out float3 forward, out float3 upVector);
            float3 right = Vector3.Cross(forward, upVector).normalized;

            Vector3 worldLeft = position + (-right * width * 0.5f);
            Vector3 worldRight = position + (right * width * 0.5f);

            leftPoint = transform.InverseTransformPoint(worldLeft);
            rightPoint = transform.InverseTransformPoint(worldRight);
        }

        private void BuildMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "GeneratedMesh";

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int vertexIndex = 0;
            float uvOffset = 0;

            for (int currentSplineIndex = 0; currentSplineIndex < splineContainer.Splines.Count; currentSplineIndex++)
            {
                int splineOffset = segments * currentSplineIndex;
                splineOffset += currentSplineIndex;

                for (int currentSplinePoint = 1; currentSplinePoint < segments + 1; currentSplinePoint++)
                {
                    int vertexOffset = currentSplinePoint + splineOffset;

                    // top face vertices
                    Vector3 r1 = rightVertices[vertexOffset - 1];
                    Vector3 l1 = leftVertices[vertexOffset - 1];
                    Vector3 r2 = rightVertices[vertexOffset];
                    Vector3 l2 = leftVertices[vertexOffset];

                    // bottom face vertices
                    Vector3 r1b = r1 + Vector3.down * thickness;
                    Vector3 l1b = l1 + Vector3.down * thickness;
                    Vector3 r2b = r2 + Vector3.down * thickness;
                    Vector3 l2b = l2 + Vector3.down * thickness;

                    // distance between r1 and r2 for UV mapping
                    float distance = Vector3.Distance(r1, r2) / 4f;
                    float uvDistance = uvOffset + distance;

                    // top face
                    vertices.AddRange(new[] { r1, l1, r2, l2 });
                    uvs.AddRange(new[] { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1) });
                    triangles.AddRange(new[] {
                        vertexIndex + 0, vertexIndex + 2, vertexIndex + 3,
                        vertexIndex + 3, vertexIndex + 1, vertexIndex + 0
                    });

                    // bot face (flipped normals)
                    vertices.AddRange(new[] { r1b, r2b, l2b, l1b });
                    uvs.AddRange(new[] { new Vector2(uvOffset, 0), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1), new Vector2(uvOffset, 1) });
                    triangles.AddRange(new[] {
                        vertexIndex + 6, vertexIndex + 5, vertexIndex + 4,
                        vertexIndex + 4, vertexIndex + 7, vertexIndex + 6
                    });

                    // right side
                    vertices.AddRange(new[] { r1, r1b, r2b, r2 });
                    uvs.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
                    triangles.AddRange(new[] {
                        vertexIndex + 8, vertexIndex + 9, vertexIndex + 10,
                        vertexIndex + 10, vertexIndex + 11, vertexIndex + 8
                    });

                    // left side
                    vertices.AddRange(new[] { l2, l2b, l1b, l1 });
                    uvs.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
                    triangles.AddRange(new[] {
                        vertexIndex + 12, vertexIndex + 13, vertexIndex + 14,
                        vertexIndex + 14, vertexIndex + 15, vertexIndex + 12
                    });

                    vertexIndex += 16;
                    uvOffset += distance;
                }

                // front face
                Vector3 frontRightTop = rightVertices[splineOffset];
                Vector3 frontLeftTop = leftVertices[splineOffset];
                Vector3 frontRightBottom = frontRightTop + Vector3.down * thickness;
                Vector3 frontLeftBottom = frontLeftTop + Vector3.down * thickness;

                vertices.AddRange(new[] { frontRightTop, frontLeftTop, frontLeftBottom, frontRightBottom });
                uvs.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                triangles.AddRange(new[]
                {
                    vertexIndex + 0, vertexIndex + 1, vertexIndex + 2,
                    vertexIndex + 2, vertexIndex + 3, vertexIndex + 0
                });
                vertexIndex += 4;

                // back face
                Vector3 backRightTop = rightVertices[splineOffset + segments];
                Vector3 backLeftTop = leftVertices[splineOffset + segments];
                Vector3 backRightBottom = backRightTop + Vector3.down * thickness;
                Vector3 backLeftBottom = backLeftTop + Vector3.down * thickness;

                vertices.AddRange(new[] { backRightTop, backRightBottom, backLeftBottom, backLeftTop });
                uvs.AddRange(new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right });
                triangles.AddRange(new[]
                {
                    vertexIndex + 0, vertexIndex + 1, vertexIndex + 2,
                    vertexIndex + 2, vertexIndex + 3, vertexIndex + 0
                });
                vertexIndex += 4;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;
        }

        [Button("Rebuild Mesh Collider")]
        public void RebuildMeshCollider()
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                return;
            }
            gameObject.AddComponent<MeshCollider>();
        }
#endif
    }
}
