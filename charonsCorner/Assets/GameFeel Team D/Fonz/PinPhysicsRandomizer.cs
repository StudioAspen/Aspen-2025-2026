using UnityEngine;
using static Unity.Cinemachine.LensSettings;

/// <summary>
/// Codecks Card Summary:
/// Pins should have somewhat randomized physics once hit. This means that after (and only after) the player collides with them, certain factors should be randomized between two values.
/// Factors are how far the pin flies and how heavy/how affected by gravity it is.
/// Some pins should basically just fall over, others should go flying, some should fly off but fall quickly, etc.
/// Unsure what the upper and lower values should be at the moment, so please create this in a way where they can be edited on a global scale in the editor for me to tinker with.
/// </summary>

[RequireComponent(typeof(Rigidbody))]
public class PinPhysicsRandomizer : MonoBehaviour
{
    Rigidbody rb;

    private bool hasBeenHit = false;

    [Header("Randomization Ranges")]
    [SerializeField] private float minLaunchForce = 5f;
    [SerializeField] private float maxLaunchForce = 15f;

    [SerializeField] private float minGravityScale = 0.5f;
    [SerializeField] private float maxGravityScale = 2f;

    [SerializeField] private float minMassMultiplier = 0.8f;
    [SerializeField] private float maxMassMultiplier = 1.5f;

    [Header("Script References")]
    [SerializeField] private PinCrashTracker pinCrashTracker;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pinCrashTracker = GameObject.Find("Player").GetComponent<PinCrashTracker>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBeenHit)
        {
            return; // Only randomize physics on the first hit
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log(this.name + " hit by player, randomizing physics!");
            RandomizePhysics(collision);

            if(pinCrashTracker != null)
                pinCrashTracker.RegisterPinHit(); // Register the pin hit with the crash tracker
            else
                Debug.LogWarning("PinCrashTracker not found.");

            hasBeenHit = true;
        }
    }

    private void RandomizePhysics(Collision collision)
    {
        // Random launch force
        float launchForce = Random.Range(minLaunchForce, maxLaunchForce);
        // Random gravity scale (simulate by adjusting drag)
        float gravityScale = Random.Range(minGravityScale, maxGravityScale);
        // Random mass multiplier
        float massMultiplier = Random.Range(minMassMultiplier, maxMassMultiplier);

        rb.mass *= massMultiplier;

        // Adjust drag to simulate gravity scale (higher gravity scale means less drag)
        rb.AddForce(collision.contacts[0].normal * launchForce, ForceMode.Impulse);

        // Launch Away from the player
        Vector3 launchDirection = (transform.position - collision.transform.position).normalized + Vector3.up * 0.5f;
        rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
    }
}
