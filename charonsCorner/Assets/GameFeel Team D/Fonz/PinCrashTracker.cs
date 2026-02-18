using UnityEngine;

public class PinCrashTracker : MonoBehaviour
{
    [SerializeField] private float pinHitTimeWindow = 2f; // Time window to track pin hits
    [SerializeField] private int pinHitCount = 0; // Count of pins hit within the time window

    public void RegisterPinHit()
    {
        pinHitCount++;
        Invoke(nameof(ResetPinHitCount), pinHitTimeWindow); // Reset count after the time window
    }

    private void ResetPinHitCount()
    {
        pinHitCount = 0;
    }

    public int GetPinHitCount() => pinHitCount;
}
