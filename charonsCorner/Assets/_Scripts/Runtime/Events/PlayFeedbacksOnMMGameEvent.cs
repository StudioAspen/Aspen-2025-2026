using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A script that listens for a specific MMGameEvent and plays all MMF_Player components attached to its GameObject.
    /// </summary>
    public class PlayFeedbacksOnMMGameEvent : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Tooltip("The name of the MMGameEvent that will trigger the feedbacks.")]
        [SerializeField] private string _eventName;

        private MMF_Player[] _feedbacks;

        protected virtual void Awake()
        {
            _feedbacks = GetComponents<MMF_Player>();
        }

        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }

        /// <summary>
        /// When the event we're listening for happens, we play our feedbacks.
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if (gameEvent.EventName == _eventName)
            {
                PlayFeedbacks();
            }
        }

        /// <summary>
        /// Plays all MMF_Player feedbacks attached to this GameObject.
        /// </summary>
        public virtual void PlayFeedbacks()
        {
            if (_feedbacks == null)
            {
                return;
            }

            foreach (MMF_Player player in _feedbacks)
            {
                if (player != null)
                {
                    player.PlayFeedbacks();
                }
            }
        }
    }
}
