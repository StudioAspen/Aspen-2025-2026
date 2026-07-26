using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

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
        
        [SerializeField] private UnityEvent OnThisCheckpoint;

        public static event Action<Checkpoint> OnCheckpointHit;

        private bool _hasActivated = false;

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
            if (_hasActivated) return;
            
            _hasActivated = true;
            OnCheckpointHit?.Invoke(this);
            OnThisCheckpoint?.Invoke();
        }
    }
}
