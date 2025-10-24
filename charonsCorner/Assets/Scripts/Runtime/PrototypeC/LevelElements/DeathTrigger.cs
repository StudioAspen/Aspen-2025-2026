using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DeathTrigger : MonoBehaviour
    {
        [SerializeField] private TriggerMode triggerMode;

        public enum TriggerMode
        {
            OnEnter,
            OnExit
        }

        private void HandlePlayerDeath(Collider other)
        {
            PrototypePlayerController player = other.GetComponent<PrototypePlayerController>();
            player.Respawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            // Only handle death if we're in OnEnter mode
            if (triggerMode == TriggerMode.OnEnter)
            {
                HandlePlayerDeath(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            // Only handle death if we're in OnExit mode
            if (triggerMode == TriggerMode.OnExit)
            {
                HandlePlayerDeath(other);
            }
        }
    }
}

