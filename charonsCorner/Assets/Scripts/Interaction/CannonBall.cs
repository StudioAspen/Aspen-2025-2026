using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [Header("Projectile Parameters: ")]
    public float acceleration = -9.81f;  
    public float launchVelocity = 25f;     
    public float currentHeight = 0f;

    [Header("Shot Parameters: ")]
    public float shotAngle;
    public float shotPower;
    public float shotLoadTime;

    [Header("Positions: ")]
    public Transform cannonBase;
    public Transform cannonPillar;
    public Transform launchDirection;


    [Header("Gizmo Settings: ")]
    public int numPoints = 30;
    public float timeStep = 0.1f;

    private void OnDrawGizmos()
    {
        //Null Check:
        if (cannonBase == null) return;

        //Initial Position & Direction Parameters:
        Vector3 startPosition = cannonBase.position;
        Vector3 forward = launchDirection ? launchDirection.forward : transform.forward;

        //Convert Value into Angle:
        Quaternion angleRotation = Quaternion.AngleAxis(shotAngle, cannonBase.right);
        Vector3 direction = angleRotation * forward;

        //Save The Value for Initial Velocity In Correlation To The Direction:
        Vector3 velocity = direction.normalized * -launchVelocity;
        Vector3 previousPosition = startPosition;

        Gizmos.color = Color.yellow;

        for (int i = 1; i <= numPoints; i++)
        {
            float t = i * timeStep;

            //Calculate The Positions For Projectile Motion:
            Vector3 calculatedPostion = startPosition + (velocity * t);
            calculatedPostion.y += (0.5f * acceleration * (t * t));

            //Draw a Gizmos Line From Previous Position -> Calculated Postion:
            Gizmos.DrawLine(previousPosition, calculatedPostion);
            previousPosition = calculatedPostion;

            //Repeats numPoints Times:
        }
    }
}
