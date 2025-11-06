using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CheckpointManager : MonoBehaviour
    {
        public static CheckpointManager Instance { get; private set; }
        public Checkpoint CurrentCheckpoint { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void SetCurrentCheckpoint(Checkpoint checkpoint)
        {
            CurrentCheckpoint = checkpoint;
        }
    }
}