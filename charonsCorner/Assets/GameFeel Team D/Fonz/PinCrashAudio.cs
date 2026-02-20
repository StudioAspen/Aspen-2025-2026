using UnityEngine;

/// <summary>
/// Codecks Card Summary:
/// There are two sounds for hitting pins: Sound A is a single pin hit, and Sound B is a crash of multiple pins. Sound B should resemble the sound of getting a strike in bowling.
/// When a pin is struck, Sound A plays with small pitch variation so it doesn't get repetitive. If another pin is hit within 1 second of that first pin, Sound A plays for the second pin, but with a higher pitch (also with minor variation). This repeats for the third pin.
/// When a fourth pin is hit in the same timeframe (within one second of the first pin being hit) Sound A should fade out into Sound B.
/// Oftentimes, the player will be hitting a half dozen pins very quickly because of how they are placed in the levels. This system should be able to effectively skip Sound A in these cases, having an instant crash for large amount of pin hits.
/// </summary>

[RequireComponent(typeof(AudioSource))]
public class PinCrashAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource pinHitAudioSource; // Audio source for individual pin hits
    [SerializeField] private AudioClip pinHitClip; // Sound A for single pin hit
    [SerializeField] private AudioClip pinStrikeClip; // Sound B for multiple pins crashing

    [Header("Pin Crash Tracking Script")]
    [SerializeField] private PinCrashTracker pinCrashTracker; // Reference to the pin crash tracker to determine how many pins have been hit within the timeframe

    void Awake()
    {
        pinHitAudioSource = GetComponent<AudioSource>();
        pinCrashTracker = GameObject.Find("Player").GetComponent<PinCrashTracker>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (pinCrashTracker.GetPinHitCount() < 4)
            {
                Debug.Log("Pin hit! Playing pin hit sound.");
                PlayPinHitSound();
            }
            else
            {
                Debug.Log("Multiple pins hit! Playing strike sound.");
                PlayPinStrikeSound();
            }
        }
    }

    private void PlayPinHitSound()
    {
        pinHitAudioSource.clip = pinHitClip;
        pinHitAudioSource.pitch = Random.Range(0.95f, 1.05f); // Small pitch variation
        pinHitAudioSource.Play();
    }

    private void PlayPinStrikeSound()
    {
        pinHitAudioSource.clip = pinStrikeClip;
        pinHitAudioSource.Play();
    }
}
