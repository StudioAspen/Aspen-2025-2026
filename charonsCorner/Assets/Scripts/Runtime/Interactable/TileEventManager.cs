using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TileEventManager : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform startPosition; // Set this to the starting position in the Inspector

        private void OnEnable()
        {
            TileLogic.OnWrongTileStepped += HandleWrongTile;
        }

        private void OnDisable()
        {
            TileLogic.OnWrongTileStepped -= HandleWrongTile;
        }

        private void HandleWrongTile()
        {
            Debug.Log("Player sent back to the start!");
            if (playerTransform != null && startPosition != null)
            {
                playerTransform.position = startPosition.position;
            }
        }
    }
}