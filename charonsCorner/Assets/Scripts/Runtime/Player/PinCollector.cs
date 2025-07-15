using CharonsCorner.Utilities;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PinCollector : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public int PinCount { get; private set; }

        public void CollectPin(int amount)
        {
            PinCount += amount;
        }
    }
}
