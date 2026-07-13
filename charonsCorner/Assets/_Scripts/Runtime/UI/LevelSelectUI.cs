using MoreMountains.Feedbacks;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Left Arrow Feedbacks")]
        [SerializeField] private MMF_Player _leftEntranceFeedback;
        [SerializeField] private MMF_Player _leftExitFeedback;
        [SerializeField] private MMF_Player _leftSquishFeedback;

        [Header("Right Arrow Feedbacks")]
        [SerializeField] private MMF_Player _rightEntranceFeedback;
        [SerializeField] private MMF_Player _rightExitFeedback;
        [SerializeField] private MMF_Player _rightSquishFeedback;

        private bool _leftArrowActive;
        private bool _rightArrowActive;

        /// <summary>
        /// Updates the state of the left arrow.
        /// </summary>
        /// <param name="canPress">Whether the player can press the left arrow.</param>
        public void SetLeftArrowState(bool canPress)
        {
            if (canPress && !_leftArrowActive)
            {
                if (_leftEntranceFeedback != null) _leftEntranceFeedback.PlayFeedbacks();
                _leftArrowActive = true;
            }
            else if (!canPress && _leftArrowActive)
            {
                if (_leftExitFeedback != null) _leftExitFeedback.PlayFeedbacks();
                _leftArrowActive = false;
            }
        }

        /// <summary>
        /// Updates the state of the right arrow.
        /// </summary>
        /// <param name="canPress">Whether the player can press the right arrow.</param>
        public void SetRightArrowState(bool canPress)
        {
            if (canPress && !_rightArrowActive)
            {
                if (_rightEntranceFeedback != null) _rightEntranceFeedback.PlayFeedbacks();
                _rightArrowActive = true;
            }
            else if (!canPress && _rightArrowActive)
            {
                if (_rightExitFeedback != null) _rightExitFeedback.PlayFeedbacks();
                _rightArrowActive = false;
            }
        }

        /// <summary>
        /// Triggers the squish feedback for the left arrow.
        /// </summary>
        public void OnLeftArrowPressed()
        {
            if (_leftArrowActive && _leftSquishFeedback != null)
            {
                _leftSquishFeedback.PlayFeedbacks();
            }
        }

        /// <summary>
        /// Triggers the squish feedback for the right arrow.
        /// </summary>
        public void OnRightArrowPressed()
        {
            if (_rightArrowActive && _rightSquishFeedback != null)
            {
                _rightSquishFeedback.PlayFeedbacks();
            }
        }
    }
}
