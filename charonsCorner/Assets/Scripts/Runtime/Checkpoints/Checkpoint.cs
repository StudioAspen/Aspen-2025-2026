using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A class for a checkpoint in a level. Notifies the CheckpointManager when trigger is entered.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int _checkpointIndex;
        private Transform _respawnPoint;

        private void Awake()
        {
            if (_respawnPoint == null)  
                _respawnPoint = GetComponent<Transform>(); 
        }

        private void OnTriggerEnter(Collider other)
        {
            // notify checkpoint manager

        }

        public int GetCheckpointIndex()
        {
            return _checkpointIndex;
        }
    }
}
