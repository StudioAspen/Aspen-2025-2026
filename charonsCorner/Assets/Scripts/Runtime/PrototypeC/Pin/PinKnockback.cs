using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{

    /// <summary>
    ///  Applies knockback to a pin after it is hit by the player.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PinKnockback : MonoBehaviour
    {
        [Header("Base Impulses")]
        [SerializeField] private float _baseHorizontalImpulse = 6f;
        [SerializeField] private float _baseUpwardImpulse = 4f;
        [SerializeField] private float _baseTorqueImpulse = 0.5f; // So the pin can spin a bit

        [Header("Speed")]
        [SerializeField] private float _minSpeedForKnockback = 1f;
        [SerializeField] private float _maxSpeedForKnockback = 15f;


        [Header("Misc")]
        [SerializeField] private float _hitCooldown = 0.05f; // Prevents double-hits
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Rigidbody _rigidbody;
        private float _lastHitTime = -10f; // Start negative for first collision

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // Called when the script is loaded
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.isKinematic = false;
        }

        /// <summary>
        /// Apply forces to the pin when it collides with the player based on the player's speed.
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Player")) return;
            if (Time.time - _lastHitTime < _hitCooldown) return;
            _lastHitTime = Time.time;


            float speed = GetApproachSpeed(collision);
            float t = Mathf.InverseLerp(_minSpeedForKnockback, _maxSpeedForKnockback, speed);
            float speedScale = Mathf.Clamp01(_speedCurve.Evaluate(Mathf.Clamp01(t)));

            if (speedScale <= 0f) return;

            Vector3 away;
            if (collision.rigidbody != null)
            {
                // use RB when available
                away = transform.position - collision.rigidbody.worldCenterOfMass;
            }
            else
            {
                away = transform.position - collision.transform.position;
            }
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f)
            {
                // As a fallback, push along the contact normal
                away = collision.GetContact(0).normal;
                away.y = 0f;
            }
            away = away.sqrMagnitude > 0f ? away.normalized : Vector3.forward;

            Vector3 impulse = away * (_baseHorizontalImpulse * speedScale) + Vector3.up * (_baseUpwardImpulse * speedScale);
            _rigidbody.AddForce(impulse, ForceMode.Impulse);

            if (_baseTorqueImpulse > 0f)
            {
                Vector3 torqueAxis = Vector3.Cross(away, Vector3.up);
                if (torqueAxis.sqrMagnitude < 0.0001f) torqueAxis = Vector3.right;
                _rigidbody.AddTorque(torqueAxis.normalized * (_baseTorqueImpulse * speedScale), ForceMode.Impulse);
            }
        }

        /// <summary>
        /// Simply returns the speed of an object that has collided with this object.
        /// </summary>
        /// <param name="collision"></param>
        /// <returns></returns>
        private float GetApproachSpeed(Collision collision)
        {
            if (collision.rigidbody != null) return collision.rigidbody.linearVelocity.magnitude;

            return collision.relativeVelocity.magnitude;
        }
    }
}