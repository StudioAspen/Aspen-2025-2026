using MoreMountains.Tools;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Activates or deactivates a specified game object when a specific MMGameEvent is triggered.
    /// </summary>
    public class OnMMGameEvent : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        public enum EventAction { Activate, Deactivate }

        [Tooltip("The name of the MMGameEvent that will trigger the action.")]
        [SerializeField] private string _eventName;

        [Tooltip("The action to perform on the target game object.")]
        [SerializeField] private EventAction _action = EventAction.Deactivate;

        [Tooltip("The game object to activate or deactivate.")]
        [SerializeField] private GameObject _targetObject;

        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == _eventName && _targetObject != null)
            {
                _targetObject.SetActive(_action == EventAction.Activate);
            }
        }
    }
}
