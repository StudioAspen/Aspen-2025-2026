using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    private Vector3 _springPosition;
    private Vector3 _springVelocity;

    [Header
        (
        "Damping Ratio Settings: \n" +
        "0 = NO OSCILLATION \n" +
        "0.1 - 0.3 = UNDERDAMPED \n" +
        "0.5 - 1.0 = CRITICALLY DAMPED \n" +
        "1.0 - 2.0 = OVERDAMPED"
        )]
    [Tooltip
        (
        "Controls how much the spring resists motion.\n" +
         "0 = no damping (very bouncy), 1 = critically damped (smooth), >1 = overdamped (sluggish)."
        )]
    [Range(0f, 2f)]
    [SerializeField] private float dampingRatio = 0.5f;
    [Space]

    [Header("Oscillation Speed: ")]
    [SerializeField] private float frequency = 18f;
    [Space]

    [Header("Spring Offset Settings:")]
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;

    public void Initialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        transform.localPosition = Vector3.zero;


        //Spring Logic, No Camera Behavior Yet:
        Spring(ref _springPosition, ref _springVelocity, transform.position, dampingRatio, frequency, deltaTime);

        var localSpringPosition = _springPosition - transform.position;

        //How Far Above or Below Camera Spring is Relative to Camera:
        var springHeight = Vector3.Dot(localSpringPosition, up);

        //Apply To Camera:
        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
        transform.localPosition = localSpringPosition * linearDisplacement;
    }

    //Spring Visualization:
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, _springPosition);
        Gizmos.DrawSphere(_springPosition, 0.1f);
    }


    //NOTES: angularFrequency = Oscilation Speed.        [Higher Value, Faster Bounce Completion]
    //NOTES: dampingRatio = Oscilation Resistance.       [Lower Value = More Bounce, Higher = More Stability]
    public void Spring(ref Vector3 currentPosition, ref Vector3 currentVelocity, Vector3 targetPosition, float dampingRatio, float angularFrequency, float timeStep)
    {
        var f = 1.0f + 2.0f * timeStep * dampingRatio * angularFrequency;
        var oo = angularFrequency * angularFrequency;
        var hoo = timeStep * oo;
        var hhoo = timeStep * hoo;
        var detInv = 1.0f / (f + hhoo);
        var detX = f * currentPosition + timeStep * currentVelocity + hhoo * targetPosition;
        var detV = currentVelocity + hoo * (targetPosition - currentPosition);
        currentPosition = detX * detInv;
        currentVelocity = detV * detInv;
    }
}
