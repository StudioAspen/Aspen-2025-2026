using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        [Header("References")]
        [SerializeField, Required] private DialogueBacklog _backlog;
        [SerializeField, Required] private AnimancerComponent _animator;
        [SerializeField, Required] private DialogueOpener _dialogueOpener;

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
            DialogueOpenerSO currentOpener = _backlog.GetCurrentDialogueOpener();
            if (currentOpener == null)
            {
                _dialogueOpener.StartOpener(_backlog.DefaultCharonDialogueOpener);
                return;
            }

            _dialogueManager.SetOwner(this);
            _dialogueManager.SetBacklog(_backlog);
            _dialogueOpener.StartOpener(currentOpener);
            _dialogueManager.SetReturnAction(StartCharonDialogue);
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            if (_dialogueManager.Owner != this)
                return;
            
            _animator.Play(_reactionAnimations[opener.Reaction], _animatorFadeDuration);
            opener.ApplyEffects();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            if (_dialogueManager.Owner != this)
                return;
            
            _animator.Play(_reactionAnimations[dialogue.Reaction], _animatorFadeDuration);
            dialogue.ApplyEffects();
        }

        private void DialogueManager_OnDialogueEnded()
        {
            if (_dialogueManager.Owner != this)
                return;
            
            _animator.Play(_reactionAnimations[DialogueReaction.Idle], _animatorFadeDuration);
        }
    }
}
