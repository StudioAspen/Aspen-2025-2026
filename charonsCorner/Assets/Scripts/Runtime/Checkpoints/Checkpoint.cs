using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A class for a checkpoint in a level. Notifies the CheckpointManager when trigger is entered.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [field: SerializeField] public int CheckpointIndex { get; private set; }
        [field: SerializeField] public string CheckpointName { get; private set; }
        [field: SerializeField, Required] public Transform RespawnPoint { get; private set; }

        public static event Action<Checkpoint> OnCheckpointHit;

        private void Awake()
        {
            if (RespawnPoint == null)  
                RespawnPoint = GetComponent<Transform>(); 
        }

        private void OnTriggerEnter(Collider other)
        {
            // notify checkpoint manager
            if (!other.CompareTag("Player")) return;
            Activate();
        }

        [Button("Activate", ButtonSizes.Large)]
        public void Activate()
        {
            OnCheckpointHit?.Invoke(this);
        }
    }
}
