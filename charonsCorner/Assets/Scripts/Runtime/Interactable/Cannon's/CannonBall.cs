using UnityEngine;

namespace CharonsCorner.Runtime
{
    public enum CannonActivationMode
    {
        Touch,          
        InteractButton
    }

    public class CannonBall : MonoBehaviour
    {

        [Header("Projectile Parameters")]
        public float acceleration = -9.81f;
        public float launchVelocity = 25f;
        public float currentHeight = 0f;

        [Header("Shot Parameters")]
        public float shotAngle = 45f;
        public float shotPower = 1f;
        public float shotLoadTime = 1f;

        [Header("Pillar Movement")]
        public bool movingPillar = false;
        public float shotAngleMin = 20f;
        public float shotAngleMax = 70f;
        public float pillarSpeed = 0.5f;

        [HideInInspector]
        public float currentShotAngle;

        [Header("Transforms")]
        public Transform cannonBase;
        public Transform cannonPillar;
        public Transform launchDirection;

        [Header("Activation Mode")]
        public CannonActivationMode activationMode = CannonActivationMode.InteractButton;

        [Header("Gizmos")]
        public int numPoints = 100;
        public float timeStep = 0.1f;


        private void OnDrawGizmos()
        {
            if (cannonBase == null) return;
            Vector3 startPosition = cannonBase.position;
            Vector3 forward = launchDirection ? launchDirection.forward : transform.forward;

            Quaternion angleRotation = Quaternion.AngleAxis(shotAngle, cannonBase.right);
            Vector3 direction = angleRotation * forward;
            Vector3 velocity = direction.normalized * -launchVelocity;
            Vector3 previousPosition = startPosition;

            Gizmos.color = Color.yellow;
            for (int i = 1; i <= numPoints; i++)
            {
                float t = i * timeStep;
                Vector3 calculatedPosition = startPosition + (velocity * t);
                calculatedPosition.y += (0.5f * acceleration * (t * t));
                Gizmos.DrawLine(previousPosition, calculatedPosition);
                previousPosition = calculatedPosition;
            }
        }
    }
}
