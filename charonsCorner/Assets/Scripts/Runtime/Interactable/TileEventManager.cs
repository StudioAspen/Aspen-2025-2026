using UnityEngine;
using System.Collections.Generic;

namespace CharonsCorner.Runtime
{
    public class TileEventManager : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform startPosition;

        [Header("Tile Light Settings")]
        [SerializeField] private List<TileLogic> tiles;

        private void OnEnable()
        {
            TileLogic.OnWrongTileStepped += HandleWrongTile;
        }

        private void OnDisable()
        {
            TileLogic.OnWrongTileStepped -= HandleWrongTile;
        }

        private void Start()
        {
            // Turn on all tile lights once at scene start
            if (tiles == null || tiles.Count == 0)
            {
                tiles = new List<TileLogic>(FindObjectsOfType<TileLogic>());
            }

            foreach (var tile in tiles)
            {
                if (tile != null)
                    tile.SetLightState(true);
            }
        }

        private void HandleWrongTile()
        {
            Debug.Log("Player stepped on a wrong tile sresetting player position");

            // Move player back to start
            if (playerTransform != null && startPosition != null)
                playerTransform.position = startPosition.position;

            // Optionally, you could turn all lights back on again
            foreach (var tile in tiles)
                tile?.SetLightState(true);
        }
    }
}
