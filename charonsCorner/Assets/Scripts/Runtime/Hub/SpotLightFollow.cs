using UnityEngine;

public class SpotLightFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = false;
    [SerializeField] private bool _followZ = false;
    [SerializeField] private bool _relative = false;

    private float _initialX;
    private float _initialY;
    private float _initialZ;

    private Vector3 _targetInitialPosition;

    void Start()
    {
        Vector3 position = transform.position;
        _initialX = position.x;
        _initialY = position.y;
        _initialZ = position.z;

        if (_target != null)
        {
            _targetInitialPosition = _target.position;
        }
    }

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position;

        float newX = _initialX;
        float newY = _initialY;
        float newZ = _initialZ;

        if (_relative)
        {
            Vector3 targetDisplacement = targetPosition - _targetInitialPosition;
            if (_followX) newX += targetDisplacement.x;
            if (_followY) newY += targetDisplacement.y;
            if (_followZ) newZ += targetDisplacement.z;
        }
        else
        {
            if (_followX) newX = targetPosition.x;
            if (_followY) newY = targetPosition.y;
            if (_followZ) newZ = targetPosition.z;
        }

        transform.position = new Vector3(newX, newY, newZ);
    }
}
