using UnityEngine;
using MoreMountains.Tools;

namespace CharonsCorner.Runtime
{
    public class CallGameEventOnStart : MonoBehaviour
    {
        [SerializeField] private string _eventName;

        private void Start()
        {
            if (!string.IsNullOrEmpty(_eventName))
            {
                MMGameEvent.Trigger(_eventName);
            }
        }
    }
}
