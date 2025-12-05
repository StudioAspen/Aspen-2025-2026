using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    public class ProjectileThrower : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

        [SerializeField] private float _throwForce = 10f;
        [SerializeField] private float _upwardForce = 4f;
        [SerializeField] private float _throwTimer = 1f;
        private float _timer;


        public void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _throwTimer)
            {
                _timer = 0f;
                Throw();
            }
        }
        
        public void Throw()
        {
            GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            projectile.transform.eulerAngles = new Vector3(0f, projectile.transform.eulerAngles.y, 0f);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();

            // Create an arc by adding forward + upward force
            Vector3 force = _firePoint.forward * _throwForce + Vector3.up * _upwardForce;

            rb.AddForce(force, ForceMode.VelocityChange);
        }
        
        void OnDrawGizmos()
        {
            if (_firePoint == null)
                return;

            Gizmos.color = Color.red;

            Vector3 startPos = _firePoint.position;

            // Same force you use to throw
            Vector3 initialVelocity = _firePoint.forward * _throwForce + Vector3.up * _upwardForce;

            // How many points to draw
            int steps = 30;

            float timeStep = 0.1f;
            Vector3 previousPoint = startPos;

            for (int i = 1; i <= steps; i++)
            {
                float t = i * timeStep;

                // Physics equation: x(t) = x0 + v0 * t + 0.5 * g * t²
                Vector3 nextPoint = startPos + initialVelocity * t + 0.5f * Physics.gravity * t * t;

                Gizmos.DrawLine(previousPoint, nextPoint);
                previousPoint = nextPoint;
            }
        }

        
    }
}
