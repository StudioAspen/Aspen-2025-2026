using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using MoreMountains.Tools;

public class SplineModelFactoryLine : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private GameObject prefab;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool reverse = false;
    [SerializeField] private float scale = 1f;
    [SerializeField] private MMTweenType moveTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
    [SerializeField] private bool startPopulated = false;

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;

    private float _spawnTimer;

    private class MovingObject
    {
        public GameObject Instance;
        public float Distance;
    }

    private List<MovingObject> _activeObjects = new List<MovingObject>();

    void Start()
    {
        if (startPopulated && isActive)
        {
            PopulateSpline();
        }
    }

    void Update()
    {
        if (!isActive) return;

        UpdateSpawning();
        UpdateMovement();
    }

    private void UpdateSpawning()
    {
        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= spawnInterval)
        {
            _spawnTimer = 0f;
            Spawn();
        }
    }

    private void PopulateSpline()
    {
        if (prefab == null || splineContainer == null || spawnInterval <= 0f) return;

        float splineLength = splineContainer.CalculateLength();
        if (splineLength <= 0) return;

        // distance = speed * time
        // The distance between two objects is speed * spawnInterval
        float distanceBetweenObjects = speed * spawnInterval;
        if (distanceBetweenObjects <= 0) return;

        float currentDistance = 0f;
        while (currentDistance < splineLength)
        {
            SpawnAt(currentDistance);
            currentDistance += distanceBetweenObjects;
        }

        // Set the timer to match the next expected spawn
        // If we spawned at 0, interval, 2*interval...
        // The last one was at some distance, we want to know how long since then
        _spawnTimer = 0f;
    }

    private void SpawnAt(float distance)
    {
        if (prefab == null || splineContainer == null) return;

        GameObject instance = Instantiate(prefab, splineContainer.transform);
        instance.transform.localScale = prefab.transform.localScale * scale;
        MovingObject obj = new MovingObject { Instance = instance, Distance = distance };
        _activeObjects.Add(obj);
        
        UpdateObjectPosition(obj);
    }

    private void Spawn()
    {
        SpawnAt(0f);
    }

    private void UpdateMovement()
    {
        float splineLength = splineContainer.CalculateLength();
        if (splineLength <= 0) return;

        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = _activeObjects[i];
            obj.Distance += speed * Time.deltaTime;

            if (obj.Distance >= splineLength)
            {
                Destroy(obj.Instance);
                _activeObjects.RemoveAt(i);
            }
            else
            {
                UpdateObjectPosition(obj);
            }
        }
    }

    private void UpdateObjectPosition(MovingObject obj)
    {
        float splineLength = splineContainer.CalculateLength();
        float progress = obj.Distance / splineLength;
        
        // Apply MMTween to progress
        float tweenedProgress = moveTween.Evaluate(progress);
        
        float t = reverse ? 1f - tweenedProgress : tweenedProgress;
        
        // Evaluate position on the spline in world space
        Vector3 position = splineContainer.EvaluatePosition(t);
        Vector3 forward = splineContainer.EvaluateTangent(t);
        Vector3 up = splineContainer.EvaluateUpVector(t);

        if (reverse)
        {
            forward = -forward;
        }

        obj.Instance.transform.position = position;
        if (forward != Vector3.zero)
        {
            obj.Instance.transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}
