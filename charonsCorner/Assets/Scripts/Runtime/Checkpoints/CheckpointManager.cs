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

        /// <summary>
        /// This method sets a checkpoint as active, meaning the one the player will respawn/fly back to after death.
        /// </summary>
        /// <param name="checkpoint">The checkpoint to be set.</param>
        private void SetActiveCheckpoint(Checkpoint checkpoint)
        {
            if (_currentCheckpoint == null)
            {
                _currentCheckpoint = checkpoint;
            }
            else
            {
                // can only progress forward in checkpoints
                if (_currentCheckpoint.GetCheckpointIndex() < checkpoint.GetCheckpointIndex()) 
                {
                    _currentCheckpoint = checkpoint;
                }
            }
        }
    }
}
