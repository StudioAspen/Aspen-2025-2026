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

        private void Awake()
        {
            _dialogueManager = DialogueManager.Instance;

            _dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            _dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            _dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;
        }

        private void Start()
        {
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
            
            opener.ApplyEffects();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            if (_dialogueManager.Owner != this)
                return;
            
            dialogue.ApplyEffects();
        }

        private void DialogueManager_OnDialogueEnded()
        {
            if (_dialogueManager.Owner != this)
                return;
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
    }
}
