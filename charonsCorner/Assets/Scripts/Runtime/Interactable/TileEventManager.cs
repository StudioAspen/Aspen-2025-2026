using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// TileEventManager is created to manage the correct/incorrect tile behavior for the player as they roll onto the interactable grid. 
/// When the player rolls onto a correct tile they are allowed to continue, when they roll onto an 'incorrect' tile they are auto transported back to the starting location (identified in unity)
/// It is managed on the parent object (can identify grid component on this object as well), the child object should have the tilemap and tilemap renderer
/// A second empty child object can be created for the starting position and loaded into the scene/moved around the scene where the player object will spawn to when they hit an incorrect tile.
/// This script should be used in conjunction with the TileLogic.cs script attached to the Incorrect/Correct tile prefabs. 
/// </summary>



namespace CharonsCorner.Runtime
{
    public class TileEventManager : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform startPosition;

        [Header("Tile Light Settings")]
        [SerializeField] private List<TileBoolean> tiles;

        private void OnEnable()
        {
            TileBoolean.OnWrongTileStepped += HandleWrongTile;
        }

        private void OnDisable()
        {
            TileBoolean.OnWrongTileStepped -= HandleWrongTile;
        }

        private void Start()
        {
            // Turn on all tile lights once at scene start
            if (tiles == null || tiles.Count == 0)
            {
                tiles = new List<TileBoolean>(FindObjectsByType<TileBoolean>(FindObjectsSortMode.None));
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


            foreach (var tile in tiles)
                tile?.SetLightState(true);
        }
    }
}
