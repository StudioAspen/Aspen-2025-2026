using UnityEngine;
using MoreMountains.Feedbacks;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A simple script that plays all MMF_Player feedbacks attached to this GameObject when it starts.
    /// </summary>
    public class PlayFeedbacksOnStart : MonoBehaviour
    {
        private MMF_Player[] _feedbacks;

        protected virtual void Awake()
        {
            _feedbacks = GetComponents<MMF_Player>();
        }

        protected virtual void Start()
        {
            PlayFeedbacks();
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
