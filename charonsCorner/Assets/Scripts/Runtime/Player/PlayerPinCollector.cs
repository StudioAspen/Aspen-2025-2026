using CharonsCorner.Utilities;
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

        public void CollectPin(int amount)
        {
            PinCount += amount;
            playerController.MultiplyRollSpeed(pinCollectSpeedMultiplier);
        }
    }
}
