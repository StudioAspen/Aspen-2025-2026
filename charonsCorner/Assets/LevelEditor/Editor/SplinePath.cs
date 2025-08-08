using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;
using NaughtyAttributes;

namespace CharonsCorner.LevelEditor.Editor
{
    [RequireComponent(typeof(SplineContainer))]
    [ExecuteInEditMode()]
    public class SplinePath : MonoBehaviour
    {
        private SplineContainer splineContainer;

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private float width = 1f;
        [SerializeField] private int segments = 25;

        private List<Vector3> leftVertices = new();
        private List<Vector3> rightVertices = new();

        private void Awake()
        {
            splineContainer = GetComponent<SplineContainer>();
        }

        private void OnEnable()
        {
            Spline.Changed += Spline_Changed;
        }

        private void OnDisable()
        {
            Spline.Changed -= Spline_Changed;
        }

        private void OnDrawGizmos()
        {

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
            for (int currentSegment = 0; currentSegment <= segments; currentSegment++)
            {
                float parameter = currentSegment * step;
                SampleSplineWidth(parameter, width, out Vector3 leftPoint, out Vector3 rightPoint);
                leftVertices.Add(leftPoint);
                rightVertices.Add(rightPoint);
            }
        }

        private void SampleSplineWidth(float parameter, float width, out Vector3 leftPoint, out Vector3 rightPoint)
        {
            splineContainer.Evaluate(0, parameter, out float3 position, out float3 forward, out float3 upVector);
            float3 right = Vector3.Cross(forward, upVector).normalized;
            leftPoint = position + (-right * width * 0.5f);
            rightPoint = position + (right * width * 0.5f);
        }

        private void BuildMesh()
        {
            Mesh mesh = new Mesh();
            mesh.name = "SplinePathMesh";

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int offset = 0;
            float uvOffset = 0;
            int length = leftVertices.Count;

            for (int currentSplinePoint = 0; currentSplinePoint < length; currentSplinePoint++)
            {
                if(currentSplinePoint == length - 1 && !splineContainer.Splines[0].Closed)
                    break; // Do not create triangles for the last point if the spline is not closed.

                Vector3 r1 = rightVertices[currentSplinePoint];
                Vector3 l1 = leftVertices[currentSplinePoint];

                // If we are at the last vertex, connect it to the first vertex
                Vector3 r2 = currentSplinePoint != length - 1 ? rightVertices[currentSplinePoint + 1] : rightVertices[0];
                Vector3 l2 = currentSplinePoint != length - 1 ? leftVertices[currentSplinePoint + 1] : leftVertices[0];

                offset = 4 * currentSplinePoint;

                // Triangles must follow right-hand rule where the normal is the thumb
                // Triangle r1,r2,l1
                int t1 = offset + 0;
                int t2 = offset + 2;
                int t3 = offset + 1;

                // Triangle l1,r2,l2
                int t4 = offset + 1;
                int t5 = offset + 2;
                int t6 = offset + 3;

                vertices.AddRange(new Vector3[] { r1, l1, r2, l2 });
                triangles.AddRange(new int[] { t1, t2, t3, t4, t5, t6 });

                float distance = Vector3.Distance(r1, r2) / 4f;
                float uvDistance = uvOffset + distance;
                uvs.AddRange(new Vector2[] { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1), new Vector2(uvDistance, 0), new Vector2(uvDistance, 1) });

                uvOffset += distance;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            meshFilter.mesh = mesh;
        }

        [Button("Rebuild Mesh Collider")]
        private void RebuildMeshCollider()
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if(meshCollider != null)
            {
                meshCollider.sharedMesh = meshFilter.sharedMesh;
                return;
            }
            gameObject.AddComponent<MeshCollider>();
        }
    }
}
