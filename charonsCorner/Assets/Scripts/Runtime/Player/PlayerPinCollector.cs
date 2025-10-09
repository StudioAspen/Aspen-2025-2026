using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerPinCollector : MonoBehaviour
    {
        private PlayerController playerController;

        [field: SerializeField, ReadOnly] public int PinCount { get; private set; }
        [SerializeField] private float pinCollectSpeedMultiplier = 1.05f;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }
        
        /// <summary>
        /// Collects pins and increases the player's pin count.
        /// Collecting a pin also increases the player's roll speed by a multiplier.
        /// </summary>
        /// <param name="amount"></param>
        public void CollectPin(int amount)
        {
            PinCount += amount;
        }
    }
}
