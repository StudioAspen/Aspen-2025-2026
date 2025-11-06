using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Checkpoint : MonoBehaviour
    {
        public bool IsActivated { get; private set; } = false;

        public void Activate()
        {
            IsActivated = true;
        }
    }
}