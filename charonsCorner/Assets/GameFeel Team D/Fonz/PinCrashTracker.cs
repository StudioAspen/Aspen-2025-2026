using UnityEngine;

public class PinCrashTracker : MonoBehaviour
{
    [SerializeField] private float _pinHitTimeWindow = 2f; // Time window to track pin hits
    [SerializeField] private int _pinHitCount = 0; // Count of pins hit within the time window

    public void RegisterPinHit()
    {
        _pinHitCount++;
        Invoke(nameof(ResetPinHitCount), _pinHitTimeWindow); // Reset count after the time window
    }

    private void ResetPinHitCount()
    {
        if (_pinHitCount > 0)
            _pinHitCount--;
    }

    public int GetPinHitCount() => _pinHitCount;
}
