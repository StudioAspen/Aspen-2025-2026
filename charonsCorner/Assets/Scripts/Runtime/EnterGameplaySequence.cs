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

        private bool _hasBeenCalled;

        /// <summary>
        /// Plays the appropriate sequence based on whether this is the first call in the current scene.
        /// </summary>
        public void PlaySequence()
        {
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
