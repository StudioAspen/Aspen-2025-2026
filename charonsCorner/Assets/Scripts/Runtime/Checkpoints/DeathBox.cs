using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This class for a "death" box that respawns the player by dragging them back to the track.
    /// This requires a Trigger Collider and works in tandem with the PlayerDeathHandler and Checkpoint system.
    /// </summary>
    [RequireComponent (typeof(Collider))]
    public class DeathBox : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CheckpointManager _checkpointManager;
        [SerializeField] private Transform _respawnPoint;

        private void Start()
        {
            if (_checkpointManager == null)
            {
                _checkpointManager = FindAnyObjectByType<CheckpointManager>();
            }        
        }

        private void OnTriggerEnter(Collider other)
        {
            // notify player death handler
            if (!other.CompareTag("Player")) return;
            var deathHandler = other.GetComponentInParent<PlayerDeathHandler>();
            if (!deathHandler) return;
            
            if (_checkpointManager != null)
            {
                _respawnPoint = _checkpointManager.CurrentCheckpoint.RespawnPoint;
                deathHandler.RespawnTo(_respawnPoint);
            }
        }

    }
}
