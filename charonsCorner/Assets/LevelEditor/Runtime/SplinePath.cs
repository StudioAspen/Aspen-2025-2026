using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Serialization;
using UnityEngine.Splines.Interpolators;

namespace CharonsCorner.LevelEditor
{
    [Serializable]
    public struct Intersection
    {
        public List<Junction> junctions;
    }

    [Serializable]
    public struct Junction
    {
        public int splineIndex;
        public int knotIndex;
        public SplineContainer spline;

        public Junction(int argSplineIndex, int argKnotIndex, SplineContainer argSpline)
        {
            splineIndex = argSplineIndex;
            knotIndex = argKnotIndex;
            spline = argSpline;
        }
    }
    
    /// <summary>
    /// The path along a spline.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SplineContainer))]
    public class SplinePath : MonoBehaviour
    {
        [HideInInspector] public SplineContainer splineContainer;
        [HideInInspector] public MeshFilter pathMeshFilter;

        [Header("Config")] 
        [SerializeField, Min(0.01f)] private float _segmentLength = 3;

        [SerializeField] private List<Intersection> _intersections = new List<Intersection>();

        /// <summary>
        /// Add an intersection to the spline path
        /// </summary>
        /// <param name="intersection">The intersection being added.</param>
        public void AddIntersection(Intersection intersection)
        {
            _intersections.Add(intersection);
        }

        /// <summary>
        /// Rebuild the mesh and mesh collider for the path.
        /// </summary>
        public void CookSplinePath()
        {
            // Return if there are no splines
            if (splineContainer.Splines.Count == 0)
            {
                return;
            }
            
            // Create scale spline data on new splines
            foreach (Spline spline in splineContainer.Splines)
            {
                SplineData<float> xScaleData;
                SplineData<float> yScaleData;
                if (spline.TryGetFloatData(SplineScale.X_SCALE_KEY, out xScaleData) == false)
                {
                    xScaleData = spline.GetOrCreateFloatData(SplineScale.X_SCALE_KEY);
                    xScaleData.PathIndexUnit = PathIndexUnit.Normalized;
                    xScaleData.Add(new DataPoint<float>(0, 10));
                    xScaleData.Add(new DataPoint<float>(1, 10));
                }
                if (spline.TryGetFloatData(SplineScale.Y_SCALE_KEY, out yScaleData) == false)
                {
                    yScaleData = spline.GetOrCreateFloatData(SplineScale.Y_SCALE_KEY);
                    yScaleData.PathIndexUnit = PathIndexUnit.Normalized;
                    yScaleData.Add(new DataPoint<float>(0, 1));
                    yScaleData.Add(new DataPoint<float>(1, 1));
                }
            }
            
            RebuildMesh();
            RebuildMeshCollider();
        }

        /// <summary>
        /// Get the left and right points along the spline.
        /// </summary>
        /// <param name="splineIndex">The spline index.</param>
        /// <param name="t">The distance along the spline (0, 1)</param>
        /// <param name="width">The width of the left and right points.</param>
        /// <param name="leftPoint">The left point on the sampled spline.</param>
        /// <param name="rightPoint">The right point on the sampled spline.</param>
        private void SampleSplineWidth(int splineIndex, float t, out Vector3 leftPoint, out Vector3 rightPoint)
        {
            splineContainer.Evaluate(splineIndex, t, out float3 position, out float3 forward, out float3 upVector);
            
            // Approximate tangent if forward is a zero vector (When spline point is set to linear)
            if (((Vector3)forward).sqrMagnitude < 1e-6f)    
            {
                // fallback: sample slightly before/after t to approximate tangent
                float dt = 0.001f; // small step
                float t0 = Mathf.Max(0, t - dt);
                float t1 = Mathf.Min(1, t + dt);

                splineContainer.Evaluate(splineIndex, t0, out float3 p0, out _, out _);
                splineContainer.Evaluate(splineIndex, t1, out float3 p1, out _, out _);
                forward = (Vector3)(p1 - p0);
            }
            
            float3 right = Vector3.Cross(forward, upVector).normalized;
            Spline spline = splineContainer.Splines[splineIndex];
            SplineData<float> xScaleData = spline.GetOrCreateFloatData(SplineScale.X_SCALE_KEY);
            float width = xScaleData.Evaluate(spline, t, PathIndexUnit.Normalized, new LerpFloat());
            
            Vector3 worldLeft = position + (-right * width * 0.5f);
            Vector3 worldRight = position + (right * width * 0.5f);
            
            leftPoint = transform.InverseTransformPoint(worldLeft);
            rightPoint = transform.InverseTransformPoint(worldRight);
        }

        /// <summary>
        /// Rebuild the path's mesh geometry based on the spline
        /// </summary>
        private void RebuildMesh()
        {
            // --------------------------------------------------------------------------------
            // Initialize mesh
            
            Mesh mesh = pathMeshFilter.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh();
                mesh.name = "PathMesh";
            }
            
            mesh.Clear();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // --------------------------------------------------------------------------------
            // Build main spline path
            
            int offset = 0;
            
            for (int currentSplineIndex = 0; currentSplineIndex < splineContainer.Splines.Count; currentSplineIndex++)
            {
                // Skip spline if it doens't have at least two knots
                if (splineContainer.Splines[currentSplineIndex].Knots.Count() <= 1)
                {
                    continue;
                }
                
                // Get the left and right vertices for path mesh
                List<Vector3> leftVertices = new List<Vector3>();
                List<Vector3> rightVertices = new List<Vector3>();
                
                float splineLength = splineContainer.CalculateLength(currentSplineIndex);
                int numSegments = Mathf.Max(1, (int)Mathf.Ceil(splineLength / _segmentLength));

                for (int i = 0; i < numSegments + 1; i++)
                {
                    float t = i / (float)numSegments;
                    SampleSplineWidth(currentSplineIndex, t, out Vector3 leftPoint, out Vector3 rightPoint);
                    leftVertices.Add(leftPoint);
                    rightVertices.Add(rightPoint); 
                }

                // Create faces for each segment
                for (int i = 0; i < numSegments; i++)
                {
                    Vector3 r1 = rightVertices[i];
                    Vector3 l1 = leftVertices[i];
                    Vector3 r2 = rightVertices[i + 1];
                    Vector3 l2 = leftVertices[i + 1];
                    
                    float t = i / (float)numSegments;
                    SplineData<float> yScaleData = splineContainer.Splines[currentSplineIndex].GetOrCreateFloatData(SplineScale.Y_SCALE_KEY);
                    float height = yScaleData.Evaluate(splineContainer.Splines[currentSplineIndex], t, PathIndexUnit.Normalized, new LerpFloat());
                    Vector3 r3 = r1 + Vector3.down * height;
                    Vector3 l3 = l1 + Vector3.down * height;
                    Vector3 r4 = r2 + Vector3.down * height;
                    Vector3 l4 = l2 + Vector3.down * height;

                    // Top face
                    vertices.AddRange(new[] { r1, l1, r2, l2 }); 
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });

                    
                    triangles.AddRange(new[] {
                        offset + 0, offset + 2, offset + 3,
                        offset + 3, offset + 1, offset + 0
                    });

                    // Bottom face
                    vertices.AddRange(new[] { r3, r4, l4, l3 });
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });
                    triangles.AddRange(new[] {
                        offset + 6, offset + 5, offset + 4,
                        offset + 4, offset + 7, offset + 6
                    });

                    // Right side
                    vertices.AddRange(new[] { r1, r3, r4, r2 });
                    uvs.AddRange(new[] {Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });

                    triangles.AddRange(new[] {
                        offset + 8, offset + 9, offset + 10,
                        offset + 10, offset + 11, offset + 8
                    });

                    // Left side
                    vertices.AddRange(new[] { l2, l4, l3, l1 });
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });
                    triangles.AddRange(new[] {
                        offset + 12, offset + 13, offset + 14,
                        offset + 14, offset + 15, offset + 12
                    });
                    
                    offset += 16;
                }
            }
            
            // --------------------------------------------------------------------------------
            // Build intersections
            foreach (Intersection intersection in _intersections)
            {
                // Initialize intersection
                int count = 0;
                List<Vector3> points = new List<Vector3>();
                Vector3 center = new Vector3();
                
                // Calculate the points and center for the junction
                foreach (Junction junction in intersection.junctions)
                {
                    float t = junction.knotIndex == 0 ? 0.0f : 1.0f;
                    SampleSplineWidth(junction.splineIndex, t, out Vector3 leftPoint, out Vector3 rightPoint);
                    
                    points.Add(leftPoint);
                    points.Add(rightPoint);
                    center += leftPoint;
                    center += rightPoint;
                    count++;
                }

                center /= points.Count;

                // Sort the points before generating geometry so that faces are built in the correct order
                points.Sort((x, y) =>
                {
                    Vector3 xDir = (x - center).normalized;
                    Vector3 yDir = (y - center).normalized;

                    float xAngle = Vector3.SignedAngle(center.normalized, xDir, Vector3.up);
                    float yAngle = Vector3.SignedAngle(center.normalized, yDir, Vector3.up);

                    if (xAngle > yAngle)
                    {
                        return 1;
                    }
                    if (xAngle < yAngle)
                    {
                        return -1;
                    }
                    return 0;
                });

                // Create the geometry for the intersection
                for (int i = 1; i <= points.Count; i++)
                {
                    // Get top and bottom vertices
                    Vector3 bottomCenter = center + Vector3.down;
                    Vector3 point1 = points[i - 1];
                    Vector3 point2;
                    if (i == points.Count)
                    {
                        point2 = points[0];
                    }
                    else
                    {
                        point2 = points[i];
                    }
                    Vector3 point3 = point1 + Vector3.down;
                    Vector3 point4 = point2 + Vector3.down;
                    
                    
                    // Top face
                    vertices.AddRange(new[] { center, point1, point2});
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero });
                    triangles.AddRange(new[] {
                        offset + 0, offset + 1, offset + 2,
                    });
                    
                    // Bottom face
                    vertices.AddRange(new[] { bottomCenter, point3, point4});
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero });
                    triangles.AddRange(new[] {
                        offset + 5, offset + 4, offset + 3,
                    });

                    // Side faces
                    vertices.AddRange(new[] {point1, point2, point3, point4});
                    uvs.AddRange(new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero });
                    triangles.AddRange(new[]
                    {
                        offset + 9, offset + 7, offset + 6,
                        offset + 6, offset + 8, offset + 9
                    });

                    offset += 10;
                }
            }
            
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
                
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
        }
        
        /// <summary>
        /// Rebuild the meshes collider.
        /// </summary>
        public void RebuildMeshCollider()
        {
            Mesh pathMesh = pathMeshFilter.sharedMesh;
            Debug.Log(pathMesh.vertices.Length);
            if (pathMesh != null && pathMesh.vertices.Length == 0)
            {
                return;
            }
            
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = pathMesh;
                return;
            }
            gameObject.AddComponent<MeshCollider>();
        }
    }
}
