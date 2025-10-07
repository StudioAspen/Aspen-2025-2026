using UnityEngine;

/// <summary>
///  Applies knockback to a pin after it is hit by the player.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PinKnockback : MonoBehaviour
{
    [Header("Pin Knockback")]
    [SerializeField] private float _horizontalImpulse = 8f;
    [SerializeField] private float _upwardImpulse = 3f;
    [SerializeField] private float _torqueImpulse = 2f; // So the pin can spin a bit

    [Header("Misc")]
    [SerializeField] private float _hitCooldown = 0.05f; // Prevents double-hits

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
    /// Apply forces to the pin when it collides with the player.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        if (Time.time - _lastHitTime < _hitCooldown) return;
        _lastHitTime = Time.time;

        Transform playerTransform = collision.transform;

        Vector3 away = transform.position - playerTransform.position;
        away.y = 0f;

        Vector3 impulse = away * _horizontalImpulse + Vector3.up * _upwardImpulse;
        _rigidbody.AddForce(impulse, ForceMode.Impulse);
    }
}
