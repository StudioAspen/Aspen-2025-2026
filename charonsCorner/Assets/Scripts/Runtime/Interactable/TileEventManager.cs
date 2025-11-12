using UnityEngine;
using System.Collections.Generic;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// TileEventManager is created to manage the correct/incorrect tile behavior for the player as they roll onto the interactable grid. 
    /// When the player rolls onto a correct tile they are allowed to continue, when they roll onto an 'incorrect' tile they are auto transported back to the starting location (identified in unity)
    /// It is managed on the parent object (can identify grid component on this object as well), the child object should have the tilemap and tilemap renderer
    /// An empty object can be created for the starting position and loaded into the scene/moved around the scene where the player object will spawn to when they hit an incorrect tile.
    /// This script should be used in conjunction with the TileBoolean.cs script attached to the Incorrect/Correct tile prefabs. 
    /// </summary>

    public class TileEventManager : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _startPosition;

        [Header("Tile Light Settings")]
        [SerializeField] private List<TileBoolean> _tiles;

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
            if (_tiles == null || _tiles.Count == 0) // Turn on all tile lights once at scene start
            {
                _tiles = new List<TileBoolean>(FindObjectsByType<TileBoolean>(FindObjectsSortMode.None));
            }

            foreach (var tile in _tiles)
            {
                if (tile != null)
                    tile.SetLightState(true);
            }
        }

        private void HandleWrongTile()
        {

            if (_playerTransform != null && _startPosition != null)
            {
                    if (_playerTransform.TryGetComponent(out Rigidbody rb))
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.position = _startPosition.position;
                    }
                    else
                    {
                        _playerTransform.position = _startPosition.position;
                    }
            }


            foreach (var tile in _tiles)
                tile?.SetLightState(true);
        }
    }
}
