using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// The Checkpoint Manager keeps track of the current checkpoint the player has achieved.
    /// Once reaching a checkpoint, the player cannot regress into a previous checkpoint.
    /// </summary>

    public class CheckpointManager : MonoBehaviour
    {
        private Checkpoint _currentCheckpoint;

        private void OnEnable() => Checkpoint.OnCheckpointHit += HandleCheckpoint;
        private void OnDisable() => Checkpoint.OnCheckpointHit -= HandleCheckpoint;

        /// <summary>
        /// This method sets a checkpoint as active, meaning the one the player will respawn/fly back to after death.
        /// </summary>
        /// <param name="checkpoint">The checkpoint to be set.</param>
        private void HandleCheckpoint(Checkpoint checkpoint)
        {
            if (_currentCheckpoint == null)
            {
                _currentCheckpoint = checkpoint;
                Debug.Log($"checkpoint: {_currentCheckpoint._checkpointIndex}");
            }
            else
            {
                // can only progress forward in checkpoints
                if (_currentCheckpoint._checkpointIndex < checkpoint._checkpointIndex) 
                {
                    _currentCheckpoint = checkpoint;
                    Debug.Log($"checkpoint: {_currentCheckpoint._checkpointIndex}");
                }
            }
        }
    }
}
