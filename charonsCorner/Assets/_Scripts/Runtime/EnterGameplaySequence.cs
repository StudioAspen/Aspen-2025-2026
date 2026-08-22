using MoreMountains.Feedbacks;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Calls one MMF Player sequence the first time it's called in a scene and a different one every subsequent time.
    /// </summary>
    public class EnterGameplaySequence : MonoBehaviour
    {
        [Header("Sequences")]
        [Tooltip("The sequence to play the first time this is called in the scene.")]
        [SerializeField] private MMF_Player _firstTimeSequence;
        
        [Tooltip("The sequence to play every subsequent time this is called in the scene.")]
        [SerializeField] private MMF_Player _subsequentSequence;

        [Tooltip("The sequence to play when a special cut to gameplay is requested.")]
        [SerializeField] private MMF_Player _specialCutToGameplaySequence;

        public static bool IsSpecialSequenceQueued => _useSpecialSequenceNext;

        private bool _hasBeenCalled;
        private static bool _useSpecialSequenceNext;

        /// <summary>
        /// Plays the appropriate sequence based on whether this is the first call in the current scene.
        /// </summary>
        public void PlaySequence()
        {
            if (_useSpecialSequenceNext)
            {
                PlayFeedback(_specialCutToGameplaySequence);
                _useSpecialSequenceNext = false;
                _hasBeenCalled = true;
                return;
            }

            if (!_hasBeenCalled)
            {
                PlayFeedback(_firstTimeSequence);
                _hasBeenCalled = true;
            }
            else
            {
                PlayFeedback(_subsequentSequence);
            }
        }

        /// <summary>
        /// Forces the next call to PlaySequence to use the special cut sequence.
        /// </summary>
        public static void QueueSpecialSequence()
        {
            _useSpecialSequenceNext = true;
        }

        private void PlayFeedback(MMF_Player feedback)
        {
            if (feedback != null)
            {
                feedback.Initialization();
                feedback.PlayFeedbacks();
            }
            else
            {
                Debug.LogWarning($"[EnterGameplaySequence] {nameof(MMF_Player)} is null on {gameObject.name}", this);
            }
        }
    }
}
