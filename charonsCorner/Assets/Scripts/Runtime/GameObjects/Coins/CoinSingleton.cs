using CharonsCorner.Runtime;
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinSingleton : MonoBehaviour
{
    public static CoinSingleton Instance { get; private set; }

    [Header("Coin Settings")]
    [SerializeField] private int _coinCount = 0;
    public int CoinCount => _coinCount; 
    [SerializeField] private LayerMask playerLayer; // Assign the player layer in Inspector

    public event Action<int> OnCoinCountChanged;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        _coinCount += amount;
        Debug.Log($"Coins collected: {_coinCount}");

        OnCoinCountChanged?.Invoke(_coinCount); //notifyUI
    }

}
