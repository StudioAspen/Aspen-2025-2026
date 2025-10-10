using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [Space]
    [Range(0f, 1f)]
    [SerializeField] private float walkLeanStrength = 0.075f;
    [Range(0f, 1f)]
    [SerializeField] private float slideLeanStrength = 0.2f;
    [Space]
    [SerializeField] private float leanStrengthResponse = 5f;

    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVelocity;
    private float _smoothLeanStrength;

    public void Initialize()
    {
        _smoothLeanStrength = walkLeanStrength;
    }


    public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);

        //Adjusts Damping based off current acceleration:
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decayDamping;

        _dampedAcceleration = Vector3.SmoothDamp
        (
            current: _dampedAcceleration,
            target: planarAcceleration,
            currentVelocity: ref _dampedAccelerationVelocity,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );

        //Rotation Axis Based Off Acceleration Vector:
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;

        //Reset Rotation to Parent Object:
        transform.localRotation = Quaternion.identity;

        //Smoothing:
        var targetLeanStrength = sliding ? slideLeanStrength : walkLeanStrength;
        _smoothLeanStrength = Mathf.Lerp(_smoothLeanStrength, targetLeanStrength, 1f - Mathf.Exp(-leanStrengthResponse * deltaTime));

        //Apply Rotation Axis:
        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothLeanStrength, leanAxis) * transform.rotation;
    }
}
