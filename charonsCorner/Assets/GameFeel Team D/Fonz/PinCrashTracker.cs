using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PinCrashTracker : MonoBehaviour
{
    [SerializeField] private float _pinHitTimeWindow = 2f; // Time window to track pin hits
    [SerializeField, ReadOnly] private int _pinHitCount; // Count of pins hit within the time window
    private float _hitTimer;
    
    public void RegisterPinHit()
    {
        _pinHitCount++;
        _hitTimer = _pinHitTimeWindow;
    }

    private void Update()
    {
        if (_hitTimer > 0)
        {
            _hitTimer -= Time.deltaTime;
            if (_hitTimer <= 0)
            {
                ResetPinHitCount();
            }
        }
    }

    private void ResetPinHitCount()
    {
        _pinHitCount = 0;
    }

    public int GetPinHitCount() => _pinHitCount;
}
