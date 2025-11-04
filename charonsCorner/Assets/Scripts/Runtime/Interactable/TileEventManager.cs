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
        [SerializeField] private float lightOnDelay = 0.5f; // Delay between turning on each light
        [SerializeField] private float lightOffDelay = 0.5f; // Delay between turning off each light
        [SerializeField] private float lightsOnDuration = 10f; // Duration all lights stay on before turning off

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
                playerTransform.position = startPosition.position;
            }
        }

        private IEnumerator LightSequenceLoop()
        {
            while (true)
            {
                // Turn on lights in sequence
                foreach (var tile in tiles)
                {
                    if (tile != null) // Ensure the tile is not null
                    {
                        tile.SetLightState(true); // Turn on the light
                        yield return new WaitForSeconds(lightOnDelay);
                    }
                }

                // Keep the lights on for the specified duration
                yield return new WaitForSeconds(lightsOnDuration);

                // Turn off lights in sequence
                foreach (var tile in tiles)
                {
                    if (tile != null) // Ensure the tile is not null
                    {
                        tile.SetLightState(false); // Turn off the light
                        yield return new WaitForSeconds(lightOffDelay);
                    }
                }
            }
        }
    }
}