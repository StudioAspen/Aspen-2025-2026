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
    [Range(0f, 1f)] [SerializeField] private float startPopulationPercentage = 1f;
    
    [Header("Speed Populate From Scratch")]
    [SerializeField] private bool _speedPopulateFromScratch = false;
    [SerializeField] private float _speedMultiplier = 5f;
    [SerializeField] private AnimationCurve _speedPopulateCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public void Activate() => isActive = true;
    public void Deactivate() => isActive = false;

    private float _spawnTimer;
    private bool _isSpeedPopulating;

    private class MovingObject
    {
        public GameObject Instance;
        public float Distance;
    }

    private List<MovingObject> _activeObjects = new List<MovingObject>();

    void Start()
    {
        if (isActive)
        {
            if (_speedPopulateFromScratch)
            {
                _isSpeedPopulating = true;
            }
            else if (startPopulated)
            {
                PopulateSpline();
            }
        }
    }

    void Update()
    {
        if (!isActive) return;

        float currentSpeedMultiplier = 1f;
        if (_isSpeedPopulating)
        {
            float splineLength = splineContainer.CalculateLength();
            float targetDistance = splineLength * startPopulationPercentage;
            
            // Get the distance of the first object (the one furthest along)
            float leadDistance = 0f;
            if (_activeObjects.Count > 0)
            {
                leadDistance = _activeObjects[0].Distance;
            }

            float progress = Mathf.Clamp01(leadDistance / targetDistance);
            
            // If we have reached the target percentage, end the speed populate phase
            if (progress >= 1f && leadDistance > 0)
            {
                _isSpeedPopulating = false;
            }
            else
            {
                // Animation curve evaluates progress from start (0) to end (1)
                // 0 on the curve corresponds to max speed boost, 1 corresponds to normal speed
                float curveValue = _speedPopulateCurve.Evaluate(progress);
                currentSpeedMultiplier = Mathf.Lerp(_speedMultiplier, 1f, curveValue);
            }
        }

        UpdateSpawning(currentSpeedMultiplier);
        UpdateMovement(currentSpeedMultiplier);
    }

    private void UpdateSpawning(float multiplier)
    {
        _spawnTimer += Time.deltaTime;
        float adjustedInterval = spawnInterval / multiplier;
        if (_spawnTimer >= adjustedInterval)
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

        float maxDistance = splineLength * startPopulationPercentage;
        float currentDistance = 0f;
        while (currentDistance < maxDistance)
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

    private void UpdateMovement(float multiplier)
    {
        float splineLength = splineContainer.CalculateLength();
        if (splineLength <= 0) return;

        float currentSpeed = speed * multiplier;

        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = _activeObjects[i];
            obj.Distance += currentSpeed * Time.deltaTime;

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
