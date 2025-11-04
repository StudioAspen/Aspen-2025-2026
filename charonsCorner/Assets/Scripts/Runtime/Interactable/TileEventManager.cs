using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CharonsCorner.Runtime
{
    public class TileEventManager : MonoBehaviour
    {
        [Header("Player Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform startPosition; // Set this to the starting position in the Inspector

        [Header("Tile Light Settings")]
        [SerializeField] private List<TileLogic> tiles; // List of tiles to control
        [SerializeField] private float lightOnDelay = 0.5f; // Base delay between turning on each light
        [SerializeField] private float lightOffDelay = 0.5f; // Base delay between turning off each light
        [SerializeField, Range(0.1f, 5f)] private float speedMultiplier = 1f; // Multiplier to control the speed of the light sequence

        private Coroutine lightLoopCoroutine;

        private void OnEnable()
        {
            TileLogic.OnWrongTileStepped += HandleWrongTile;

            // Start the light loop
            lightLoopCoroutine = StartCoroutine(LightSequenceLoop());
        }

        private void OnDisable()
        {
            TileLogic.OnWrongTileStepped -= HandleWrongTile;

            // Stop the light loop
            if (lightLoopCoroutine != null)
                StopCoroutine(lightLoopCoroutine);
        }

        private void HandleWrongTile()
        {
            Debug.Log("Player sent back to the start!");
            if (playerTransform != null && startPosition != null)
            {
                // Reset the player's position to the start
                playerTransform.position = startPosition.position;
            }

            // Reset the tile lights to start from the beginning
            if (lightLoopCoroutine != null)
                StopCoroutine(lightLoopCoroutine);

            // Turn off all lights immediately
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    tile.SetLightState(false);
                }
            }

            // Restart the light sequence
            lightLoopCoroutine = StartCoroutine(LightSequenceLoop());
        }

        private IEnumerator LightSequenceLoop()
        {
            while (true)
            {
                // Turn on lights in sequence
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i] != null) // Ensure the tile is not null
                    {
                        tiles[i].SetLightState(true); // Turn on the light
                        yield return new WaitForSeconds(lightOnDelay / speedMultiplier);
                    }
                }

                // Turn off lights in sequence
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i] != null) // Ensure the tile is not null
                    {
                        tiles[i].SetLightState(false); // Turn off the light
                        yield return new WaitForSeconds(lightOffDelay / speedMultiplier);
                    }
                }
            }
        }
    }
}