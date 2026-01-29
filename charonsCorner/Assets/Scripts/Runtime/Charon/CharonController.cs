using Animancer;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        [Header("References")]
        [SerializeField] private AnimancerComponent _animator;

        [Header("Config")]
        [SerializeField] private float _animatorFadeDuration = 0.2f;

        [SerializeField, SerializedDictionary("Reaction", "Animation Clip")]
        private SerializedDictionary<DialogueReaction, ClipTransition> _reactionAnimations;

        private void Awake()
        {
            _dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);

            _dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            _dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            _dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;
        }

        private void Start()
        {
            _animator.Play(_reactionAnimations[DialogueReaction.Idle], _animatorFadeDuration);
        }

        private void OnDestroy()
        {
            if(_dialogueManager != null)
            {
                _dialogueManager.OnDialogueOpenerStarted -= DialogueManager_OnDialogueOpenerStarted;
                _dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;
                _dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            _animator.Play(_reactionAnimations[opener.Reaction], _animatorFadeDuration);
            opener.ApplyEffects();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            _animator.Play(_reactionAnimations[dialogue.Reaction], _animatorFadeDuration);
            dialogue.ApplyEffects();
        }

        private void DialogueManager_OnDialogueEnded()
        {
            _animator.Play(_reactionAnimations[DialogueReaction.Idle], _animatorFadeDuration);
        }

        /// <summary>
        /// Plays the provided animation clip using Animancer.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        public void PlayAnimation(ClipTransition clip)
        {
            if (clip != null)
            {
                _animator.Play(clip, _animatorFadeDuration);
            }
        }

        /// <summary>
        /// Plays an animation based on the provided index.
        /// The index corresponds to the values in the DialogueReaction enum.
        /// </summary>
        /// <param name="index">The index of the DialogueReaction to play.</param>
        public void PlayAnimationByIndex(int index)
        {
            if (System.Enum.IsDefined(typeof(DialogueReaction), index))
            {
                DialogueReaction reaction = (DialogueReaction)index;
                if (_reactionAnimations.ContainsKey(reaction))
                {
                    _animator.Play(_reactionAnimations[reaction], _animatorFadeDuration);
                }
                else
                {
                    Debug.LogWarning($"[CharonController] Reaction '{reaction}' not found in _reactionAnimations dictionary.");
                }
            }
            else
            {
                Debug.LogWarning($"[CharonController] Animation index {index} is out of range for DialogueReaction enum.");
            }
        }
    }
}
