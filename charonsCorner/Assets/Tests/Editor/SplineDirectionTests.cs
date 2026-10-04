using CharonsCorner.LevelEditor;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using SplinePath = CharonsCorner.LevelEditor.SplinePath;

namespace CharonsCorner.Tests
{
    public class SplineDirectionTests
    {
        private GameObject _road;
        private SplineContainer _container;
        private SplinePath _path;
        private SplinePathDirection _direction;

        [SetUp]
        public void SetUp()
        {
            _road = new GameObject("Wrong-way regression road");
            _road.transform.position = new Vector3(20000, 20000, 20000);
            _container = _road.AddComponent<SplineContainer>();
            _path = _road.AddComponent<SplinePath>();
            _direction = _road.AddComponent<SplinePathDirection>();
        }

        [TearDown]
        public void TearDown()
        {
            Mesh mesh = _road.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(_road);
            if (mesh != null && !EditorUtility.IsPersistent(mesh)) Object.DestroyImmediate(mesh);
        }

        private static Spline Line(Vector3 start, Vector3 end)
        {
            var spline = new Spline();
            spline.Add(new BezierKnot((float3)start), TangentMode.Linear);
            spline.Add(new BezierKnot((float3)end), TangentMode.Linear);
            return spline;
        }

        private void CookSharedRoad()
        {
            _container.Splines = new[]
            {
                Line(new Vector3(-20, 0, 0), new Vector3(20, 0, 0)),
                Line(new Vector3(10, 4, 0), new Vector3(-10, 4, 0))
            };
            _path.CookSplinePath();
        }

        private Vector3 SampleGround(int splineIndex, float t)
        {
            _container.Evaluate(splineIndex, t, out float3 position, out _, out _);
            Physics.SyncTransforms();
            Assert.That(_road.GetComponent<MeshCollider>().Raycast(
                new Ray((Vector3)position + Vector3.up * 2, Vector3.down), out RaycastHit hit, 3), Is.True);
            Assert.That(_direction.TryGetTravelDirectionAtHit(hit, out Vector3 direction, out int selected), Is.True);
            Assert.That(selected, Is.EqualTo(splineIndex), "Must use the spline belonging to the hit triangle.");
            return direction;
        }

        [Test]
        public void GroundHitSelectsCorrectSplineInSharedMesh()
        {
            CookSharedRoad();
            Assert.That(Vector3.Dot(SampleGround(1, 0.5f), Vector3.left), Is.GreaterThan(0.99f));
            Assert.That(Vector3.Dot(SampleGround(0, 0.5f), Vector3.right), Is.GreaterThan(0.99f));
        }

        [Test]
        public void CookedSegmentMappingSurvivesScaleAndRotationChanges()
        {
            CookSharedRoad();
            _road.transform.localScale = new Vector3(0.25f, 1, 2);
            _road.transform.rotation = Quaternion.Euler(0, 37, 0);
            Vector3 expected = _road.transform.TransformVector(Vector3.left).normalized;
            Assert.That(Vector3.Dot(SampleGround(1, 0.8f), expected), Is.GreaterThan(0.99f));
        }

        [Test]
        public void BackwardMetadataChangesTravelWithoutChangingKnots()
        {
            _container.Splines = new[] { Line(Vector3.left * 20, Vector3.right * 20) };
            _path.CookSplinePath();
            var settings = new SerializedObject(_direction);
            settings.FindProperty("_defaultDirection").enumValueIndex = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(Vector3.Dot(SampleGround(0, 0.5f), Vector3.left), Is.GreaterThan(0.99f));
            Assert.That(_container.Splines[0][0].Position.x, Is.EqualTo(-20));
        }

        [Test]
        public void ClosedLoopUsesLocalTangentOnBothSidesOfSeam()
        {
            var loop = new Spline();
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4;
                loop.Add(new BezierKnot(new float3(Mathf.Cos(angle) * 20, 0, Mathf.Sin(angle) * 20)), TangentMode.Linear);
            }
            loop.Closed = true;
            _container.Splines = new[] { loop };
            _path.CookSplinePath();
            foreach (float t in new[] { 0.01f, 0.2f, 0.4f, 0.6f, 0.8f, 0.99f })
            {
                _container.Evaluate(0, t, out _, out float3 tangent, out _);
                Assert.That(Vector3.Dot(SampleGround(0, t), ((Vector3)tangent).normalized), Is.GreaterThan(0.99f));
            }
        }
    }
}
