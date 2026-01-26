using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        [Header("References")]
        [SerializeField] private AnimancerComponent _animator;
        [SerializeField] private DialogueOpenerSO _defaultCharonDialogueOpener;

        [Header("Config")]
        [SerializeField] private float _animatorFadeDuration = 0.2f;

        [SerializeField, SerializedDictionary("Reaction", "Animation Clip")]
        private SerializedDictionary<DialogueReaction, ClipTransition> _reactionAnimations;

        private void Awake()
        {
            _dialogueManager = DialogueManager.Instance;

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
        
        public void StartCharonDialogue()
        {
            if (!_dialogueManager.Backlog.HasPendingDialogue())
            {
                _dialogueManager.StartDialogueOpener(_defaultCharonDialogueOpener);
                return;
            }

            _dialogueManager.StartDialogueOpener(_dialogueManager.Backlog.GetCurrentDialogueOpener());
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
    }
}
