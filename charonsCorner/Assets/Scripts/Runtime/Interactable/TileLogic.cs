using UnityEngine;
using System;

namespace CharonsCorner.Runtime
{
    public class TileLogic : MonoBehaviour
    {
        [Header("Tile Settings")]
        public bool isCorrectTile; // Bool for swapping between correct and wrong tiles

        // Event triggered when the player steps on a wrong tile
        public static event Action OnWrongTileStepped;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out PlayerController player))
            {
                if (isCorrectTile)
                {
                    Debug.Log("Player stepped on the correct tile.");
                }
                else
                {
                    Debug.Log("Player stepped on the wrong tile!");
                    OnWrongTileStepped?.Invoke(); // Trigger the centralized event
                }
            }
        }
    }
}