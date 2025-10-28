using CharonsCorner.Runtime;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [SerializeField] private int coinValue = 1; 
    [SerializeField] private LayerMask playerLayer; // Assign the player layer in Inspector

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering collider belongs to a specific layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Coin Game Object: Trigger entered by an object on the Player layer");
            CoinSingleton.Instance.AddCoins(coinValue);
            gameObject.SetActive(false);
        }
    }

}
