using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using MoreMountains.Tools;
using CharonsCorner.Runtime;

public class SplineModelFactoryLine : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private List<SplineContainer> extraSplines = new List<SplineContainer>();
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<GameObject> extraPrefabs = new List<GameObject>();
    [SerializeField] private float speed = 1f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private bool useRandomInterval = false;
    [SerializeField] private float minSpawnInterval = 1f;
    [SerializeField] private float maxSpawnInterval = 3f;
    [SerializeField] private bool isActive = true;
    [SerializeField] private bool reverse = false;
    [SerializeField] private float scale = 1f;
    [SerializeField] private MMTweenType moveTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
    [SerializeField] private bool startPopulated = false;
    [Range(0f, 1f)] [SerializeField] private float startPopulationPercentage = 1f;
    [SerializeField] private bool randomizeIfPossible = false;
    [SerializeField] private bool useObjectPooling = true;
    [SerializeField] private bool useSetToBlack = false;

    [Header("Start Scaling")]
    [SerializeField] private bool useStartScaling = false;
    [Range(0f, 1f)] [SerializeField] private float scalingEndPercentage = 0.2f;
    [SerializeField] private AnimationCurve startScalingCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("End Scaling")]
    [SerializeField] private bool useEndScaling = false;
    [Range(0f, 1f)] [SerializeField] private float scalingStartPercentage = 0.8f;
    [SerializeField] private AnimationCurve endScalingCurve = AnimationCurve.Linear(0, 1, 1, 0);
    
    [Header("Speed Populate From Scratch")]
    [SerializeField] private bool _speedPopulateFromScratch = false;
    [SerializeField] private float _speedMultiplier = 5f;
    [SerializeField] private AnimationCurve _speedPopulateCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Pulse")]
    [SerializeField] private string pulseEventName = "Pulse";
    [SerializeField] private float pulseDuration = 2f;
    [SerializeField] private float pulseMultiplier = 2f;
    [SerializeField] private AnimationCurve pulseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

    public void Activate()
    {
        if (!isActive)
        {
            isActive = true;
            if (_speedPopulateFromScratch && _activeObjects.Count == 0)
            {
                _isSpeedPopulating = true;
            }
        }
    }

    public void Deactivate() => isActive = false;

    public void Pulse()
    {
        _pulseTimer = 0f;
        _isPulsing = true;
    }

    private float _distanceAccumulator;
    private float _currentSpawnInterval;
    private bool _isSpeedPopulating;
    private bool _lastIsActive;
    private bool _isPulsing;
    private float _pulseTimer;

    private class MovingObject
    {
        public GameObject Instance;
        public float Distance;
        public SplineContainer Container;
        public Vector3 BaseScale;
        public PoolableObject Poolable;
    }

    private List<MovingObject> _activeObjects = new List<MovingObject>();
    private List<SplineContainer> _allSplines = new List<SplineContainer>();
    private Dictionary<SplineContainer, float> _splineLengths = new Dictionary<SplineContainer, float>();
    private List<GameObject> _allPrefabs = new List<GameObject>();

    void Start()
    {
        _lastIsActive = isActive;
        _allSplines.Clear();
        _splineLengths.Clear();
        if (splineContainer != null)
        {
            _allSplines.Add(splineContainer);
            _splineLengths[splineContainer] = splineContainer.CalculateLength();
        }
        if (extraSplines != null)
        {
            foreach (var extra in extraSplines)
            {
                if (extra != null && !_allSplines.Contains(extra))
                {
                    _allSplines.Add(extra);
                    _splineLengths[extra] = extra.CalculateLength();
                }
            }
        }

        _allPrefabs.Clear();
        if (prefab != null) _allPrefabs.Add(prefab);
        if (extraPrefabs != null)
        {
            foreach (var extra in extraPrefabs)
            {
                if (extra != null)
                {
                    _allPrefabs.Add(extra);
                }
            }
        }

        SetNextSpawnInterval();

        if (isActive)
        {
            if (_speedPopulateFromScratch)
            {
                _isSpeedPopulating = true;
            }
            else if (startPopulated)
            {
                PopulateSplines();
            }
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == pulseEventName)
        {
            Pulse();
        }
    }

    void Update()
    {
        if (isActive && !_lastIsActive)
        {
            if (_speedPopulateFromScratch && _activeObjects.Count == 0)
            {
                _isSpeedPopulating = true;
            }
        }
        _lastIsActive = isActive;

        if (!isActive) return;

        float currentSpeedMultiplier = 1f;
        if (_isSpeedPopulating)
        {
            // Speed populating logic based on the main spline or first available
            SplineContainer referenceSpline = _allSplines.Count > 0 ? _allSplines[0] : null;
            if (referenceSpline != null && _splineLengths.TryGetValue(referenceSpline, out float splineLength))
            {
                float targetDistance = splineLength * startPopulationPercentage;
                
                // Get the distance of the first object (the one furthest along) on any spline
                // Or maybe just the one on the reference spline?
                // The original logic used _activeObjects[0].
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
                    float curveValue = _speedPopulateCurve.Evaluate(progress);
                    currentSpeedMultiplier = Mathf.Lerp(_speedMultiplier, 1f, curveValue);
                }
            }
        }

        if (_isPulsing)
        {
            _pulseTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(_pulseTimer / pulseDuration);
            float pulseValue = pulseCurve.Evaluate(progress);
            // Pulse multiplier goes from 1 to pulseMultiplier and back to 1 based on the curve.
            // Usually curves are 0 to 1. If EaseInOut, it goes 0 -> 1.
            // Wait, "easing it over x to a multiplier of y and then back down to it's base".
            // If the curve is 0 to 1 and then back to 0, it works.
            // If the curve is standard 0 to 1, we might need to handle the "back down" part unless the curve itself does it.
            // Most users expect a pulse curve to represent the intensity over time.
            float currentPulse = Mathf.Lerp(1f, pulseMultiplier, pulseValue);
            currentSpeedMultiplier *= currentPulse;

            if (progress >= 1f)
            {
                _isPulsing = false;
            }
        }

        float currentSpeed = speed * currentSpeedMultiplier;
        UpdateSpawning(currentSpeed);
        UpdateMovement(currentSpeed);
    }

    private void UpdateSpawning(float currentSpeed)
    {
        _distanceAccumulator += currentSpeed * Time.deltaTime;
        float requiredDistance = speed * _currentSpawnInterval;
        
        if (requiredDistance > 0 && _distanceAccumulator >= requiredDistance)
        {
            _distanceAccumulator -= requiredDistance;
            SetNextSpawnInterval();
            Spawn();
        }
    }

    private void SetNextSpawnInterval()
    {
        if (useRandomInterval)
        {
            _currentSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }
        else
        {
            _currentSpawnInterval = spawnInterval;
        }
    }

    private void PopulateSplines()
    {
        if (_allPrefabs.Count == 0 || _allSplines.Count == 0 || _currentSpawnInterval <= 0f) return;

        foreach (var container in _allSplines)
        {
            if (!_splineLengths.TryGetValue(container, out float splineLength) || splineLength <= 0) continue;

            float distanceBetweenObjects = speed * _currentSpawnInterval;
            if (distanceBetweenObjects <= 0) continue;

            float maxDistance = splineLength * startPopulationPercentage;
            float currentDistance = 0f;
            while (currentDistance < maxDistance)
            {
                SpawnAt(container, currentDistance);
                currentDistance += distanceBetweenObjects;
                
                // If we use random intervals, we should theoretically re-calculate the distance for the next object
                // during pre-population.
                if (useRandomInterval)
                {
                    float nextInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
                    distanceBetweenObjects = speed * nextInterval;
                }
            }
        }

        _distanceAccumulator = 0f;
    }

    private void SpawnAt(SplineContainer container, float distance)
    {
        if (_allPrefabs.Count == 0 || container == null) return;

        GameObject prefabToSpawn = _allPrefabs[Random.Range(0, _allPrefabs.Count)];
        GameObject instance;
        PoolableObject poolable = null;

        if (useObjectPooling && prefabToSpawn.TryGetComponent(out PoolableObject prefabPoolable))
        {
            poolable = ObjectPoolerManager.Instance.SpawnPooledObject<PoolableObject>(prefabPoolable, container.transform.position, container.transform);
            instance = poolable.gameObject;
        }
        else
        {
            instance = Instantiate(prefabToSpawn, container.transform);
        }
        
        Vector3 instanceScale = prefabToSpawn.transform.localScale * scale;
        instance.transform.localScale = instanceScale;

        if (randomizeIfPossible)
        {
            if (instance.TryGetComponent(out RandomizeActiveObject randomizer))
            {
                randomizer.Randomize();
            }
        }

        if (useSetToBlack)
        {
            if (instance.TryGetComponent(out SetToBlack setToBlack))
            {
                setToBlack.Active = true;
            }
        }

        MovingObject obj = new MovingObject { Instance = instance, Distance = distance, Container = container, BaseScale = instanceScale, Poolable = poolable };
        _activeObjects.Add(obj);
        
        UpdateObjectPosition(obj);
    }

    private void Spawn()
    {
        foreach (var container in _allSplines)
        {
            SpawnAt(container, 0f);
        }
    }

    private void UpdateMovement(float currentSpeed)
    {
        for (int i = _activeObjects.Count - 1; i >= 0; i--)
        {
            var obj = _activeObjects[i];
            obj.Distance += currentSpeed * Time.deltaTime;

            if (_splineLengths.TryGetValue(obj.Container, out float splineLength) && obj.Distance >= splineLength)
            {
                if (useObjectPooling && obj.Poolable != null)
                {
                    obj.Instance.ReturnToPool();
                }
                else
                {
                    Destroy(obj.Instance);
                }
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
        if (!_splineLengths.TryGetValue(obj.Container, out float splineLength) || splineLength <= 0) return;
        
        float progress = obj.Distance / splineLength;
        
        // Apply MMTween to progress
        float tweenedProgress = moveTween.Evaluate(progress);
        
        float t = reverse ? 1f - tweenedProgress : tweenedProgress;
        
        // Evaluate position on the spline in world space
        Vector3 position = obj.Container.EvaluatePosition(t);
        Vector3 forward = obj.Container.EvaluateTangent(t);
        Vector3 up = obj.Container.EvaluateUpVector(t);

        if (reverse)
        {
            forward = -forward;
        }

        obj.Instance.transform.position = position;
        if (forward != Vector3.zero)
        {
            obj.Instance.transform.rotation = Quaternion.LookRotation(forward, up);
        }

        float currentScaleMultiplier = 1f;

        if (useStartScaling)
        {
            if (progress <= scalingEndPercentage)
            {
                float scalingProgress = progress / scalingEndPercentage;
                currentScaleMultiplier *= startScalingCurve.Evaluate(scalingProgress);
            }
        }

        if (useEndScaling)
        {
            if (progress >= scalingStartPercentage)
            {
                float scalingProgress = (progress - scalingStartPercentage) / (1f - scalingStartPercentage);
                currentScaleMultiplier *= endScalingCurve.Evaluate(scalingProgress);
            }
        }

        obj.Instance.transform.localScale = obj.BaseScale * currentScaleMultiplier;
    }
}
