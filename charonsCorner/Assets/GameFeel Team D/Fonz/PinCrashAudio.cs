using CharonsCorner.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Codecks Card Summary:
/// There are two sounds for hitting pins: Sound A is a single pin hit, and Sound B is a crash of multiple pins. Sound B should resemble the sound of getting a strike in bowling.
/// When a pin is struck, Sound A plays with small pitch variation so it doesn't get repetitive. If another pin is hit within 1 second of that first pin, Sound A plays for the second pin, but with a higher pitch (also with minor variation). This repeats for the third pin.
/// When a fourth pin is hit in the same timeframe (within one second of the first pin being hit) Sound A should fade out into Sound B.
/// Oftentimes, the player will be hitting a half dozen pins very quickly because of how they are placed in the levels. This system should be able to effectively skip Sound A in these cases, having an instant crash for large amount of pin hits.
/// </summary>

public class PinCrashAudio : MonoBehaviour
{
    private PinCrashTracker _pinCrashTracker; // Reference to the pin crash tracker to determine how many pins have been hit within the timeframe
    
    [Header("Script References")]
    [SerializeField] private OneShotAudioPlayer _pinHitOneShotAudioPlayer;
    [SerializeField] private OneShotAudioPlayer _pinStrikeOneShotAudioPlayer;

    void Awake()
    {
        _pinCrashTracker = FindAnyObjectByType<PinCrashTracker>();
        if (_pinCrashTracker == null)
            Debug.LogWarning("PinCrashAudio: PinCrashTracker not assigned.", this);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(_pinCrashTracker == null)
        {
            Debug.LogError("PinCrashAudio: PinCrashTracker reference is missing. Cannot determine pin hit count.", this);
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            _pinCrashTracker.RegisterPinHit();

            if (_pinCrashTracker.GetPinHitCount() < 4)
            {
                PlayPinHitSound();
            }
            else
            {
                PlayPinStrikeSound();
            }
        }
    }

    private void PlayPinHitSound()
    {
        _pinHitOneShotAudioPlayer.Play();
    }

    private void PlayPinStrikeSound()
    {
        _pinStrikeOneShotAudioPlayer.Play();
    }
}
