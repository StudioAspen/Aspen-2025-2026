using UnityEngine;

public class SpotLightFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private bool _followX = true;
    [SerializeField] private bool _followY = false;
    [SerializeField] private bool _followZ = false;

    private float _initialX;
    private float _initialY;
    private float _initialZ;

    void Start()
    {
        Vector3 position = transform.position;
        _initialX = position.x;
        _initialY = position.y;
        _initialZ = position.z;
    }

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = _target.position;

        float newX = _followX ? targetPosition.x : _initialX;
        float newY = _followY ? targetPosition.y : _initialY;
        float newZ = _followZ ? targetPosition.z : _initialZ;

        transform.position = new Vector3(newX, newY, newZ);
    }
}
