using CharonsCorner.Runtime;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SoulSingleton : MonoBehaviour
{
    public static SoulSingleton Instance { get; private set; }

    [Header("Soul Settings")]
    [SerializeField] private int _soulCount = 10;
    public int SoulCount => _soulCount;
    [SerializeField] private LayerMask playerLayer; // Assign the player layer in Inspector

    public event Action<int> OnSoulCountChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }
    public void SubtractSouls(int amount)
    {
        if (amount <= 0) return;

        _soulCount -= amount;
        Debug.Log($"Coins collected: {_soulCount}");

        OnSoulCountChanged?.Invoke(_soulCount); //notifyUI
    }

}
