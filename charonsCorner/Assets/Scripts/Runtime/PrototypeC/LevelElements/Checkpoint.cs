using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Checkpoint : MonoBehaviour
    {
        [field: SerializeField] public Transform RespawnPoint { get; private set; }
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            other.GetComponent<PrototypePlayerController>().SetRespawnPoint(RespawnPoint.position);
            
        }
    }
}
