using UnityEngine;
using MoreMountains.Tools;

public class CharonTalk : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private Transform jawObject;
    [SerializeField] private AnimationCurve jawCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lerpBackSpeed = 5f;
    [SerializeField] private Vector3 rotationAxis = Vector3.right;
    [SerializeField] private float maxRotationAngle = 20f;

    private bool _isTalking = false;
    private bool _isStopping = false;
    private Quaternion _initialRotation;
    private float _currentTime = 0f;

    private void Awake()
    {
        if (jawObject != null)
        {
            _initialRotation = jawObject.localRotation;
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
        if (gameEvent.EventName == "CharonTalk")
        {
            _isTalking = true;
            _isStopping = false;
        }
        else if (gameEvent.EventName == "StopTalk")
        {
            _isTalking = false;
            _isStopping = true;
        }
    }

    private void Update()
    {
        if (jawObject == null) return;

        if (_isTalking)
        {
            _currentTime += Time.deltaTime * speed;
            // Wrap the time to keep it within the curve's range if needed, or let it repeat
            float curveTime = _currentTime % 1.0f; 
            float curveValue = jawCurve.Evaluate(curveTime);
            
            jawObject.localRotation = _initialRotation * Quaternion.AngleAxis(curveValue * maxRotationAngle, rotationAxis);
        }
        else if (_isStopping)
        {
            jawObject.localRotation = Quaternion.Lerp(jawObject.localRotation, _initialRotation, Time.deltaTime * lerpBackSpeed);
            
            if (Quaternion.Angle(jawObject.localRotation, _initialRotation) < 0.1f)
            {
                jawObject.localRotation = _initialRotation;
                _isStopping = false;
                _currentTime = 0f;
            }
        }
    }
}
