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
        [SerializeField] private float _baseHorizontalImpulse = 20f;
        [SerializeField] private float _baseUpwardImpulse = 20f;

        [Header("Speed")]
        [SerializeField] private float _minSpeedForKnockback = 1f;
        [SerializeField] private float _maxSpeedForKnockback = 50f;


        [Header("Misc")]
        [SerializeField] private float _hitCooldown = 0.05f; // Prevents double-hits
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Rigidbody _rigidbody;
        private float _lastHitTime = -10f; // Start negative for first collision
        private bool _hasBeenHit = false;
        private int _pinLayer = 7;

        // position, speedScale, lookTarget
        public static event Action<Vector3, float, Transform> OnPinHit;

        // dampen the "infinite" spinning that happens sometimes
        void FixedUpdate()
        {
            if (!_hasBeenHit) return;

            Vector3 av = _rigidbody.angularVelocity;
            
            // Kill some Y spin every step (0.0�1.0, closer to 0 = stronger damping)
            float damping = 0.9f;
            av.y *= damping;

            _rigidbody.angularVelocity = av;
        }

        // Called when the script is loaded
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            // disable physics until hit (so doesnt fall over)
            _rigidbody.useGravity = false;

            _rigidbody.centerOfMass = new Vector3(0f, -0.2f, 0f);
        }

        /// <summary>
        /// Apply forces to the pin when it collides with another collision/rigidbody based on its speed.
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            bool isPlayer = collision.collider.CompareTag("Player");
            bool isPin = collision.gameObject.layer == _pinLayer;

            if (!isPlayer && !isPin) return;

            if (Time.time - _lastHitTime < _hitCooldown) return;
            _lastHitTime = Time.time;

            // activate physics when hit
            if (!_hasBeenHit)
            {
                _hasBeenHit = true;
                _rigidbody.useGravity = true;
                _rigidbody.isKinematic = false;
                _rigidbody.WakeUp();
            }

            float speed = GetApproachSpeed(collision);
            float t = Mathf.InverseLerp(_minSpeedForKnockback, _maxSpeedForKnockback, speed);
            float speedScale = Mathf.Clamp01(_speedCurve.Evaluate(Mathf.Clamp01(t)));
            // going to use already calculated speedScale to determine the size of the pow effect, instead of calculating it again in PowEffectScript  
            OnPinHit?.Invoke(transform.position, speedScale, collision.transform);
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