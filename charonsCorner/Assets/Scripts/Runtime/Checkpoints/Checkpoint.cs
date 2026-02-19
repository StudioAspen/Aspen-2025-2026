using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A class for a checkpoint in a level. Notifies the CheckpointManager when trigger is entered.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] public int _checkpointIndex;
        [SerializeField] private Transform _respawnPoint;

        public static event Action<Checkpoint> OnCheckpointHit;

        private void Awake()
        {
            if (_respawnPoint == null)  
                _respawnPoint = GetComponent<Transform>(); 
        }

        private void OnTriggerEnter(Collider other)
        {
            // notify checkpoint manager
            if (!other.CompareTag("Player")) return;
            OnCheckpointHit?.Invoke(this);
        }
    }
}
