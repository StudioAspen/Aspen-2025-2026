using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(BoxCollider))]
    public class ZoneTeleporter : MonoBehaviour
    {
        public LayerMask playerLayerMask;

        private void Awake()
        {
            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayerMask.value) != 0)
            {
                var player = other.GetComponent<GameplayPlayerController>();
                var checkpoint = CheckpointManager.Instance.CurrentCheckpoint;
                if (player != null && checkpoint != null)
                {
                    player.transform.position = checkpoint.transform.position;
                    // Optionally reset velocity, etc.
                    
                }
            }
        }
    }
}