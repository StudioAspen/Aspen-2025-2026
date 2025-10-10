using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(BoxCollider))]
    public class CheckpointZone : MonoBehaviour
    {
        public LayerMask playerLayerMask;
        [SerializeField] private Checkpoint checkpoint;

        private void Awake()
        {
            var box = GetComponent<BoxCollider>();
            box.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayerMask.value) != 0)
            {
                if (checkpoint != null)
                {
                    checkpoint.Activate();
                    CheckpointManager.Instance.SetCurrentCheckpoint(checkpoint);
                }
            }
        }
    }
}