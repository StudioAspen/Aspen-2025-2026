using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        [Header("References")]
        [SerializeField, Required] private DialogueBacklog _backlog;
        [SerializeField, Required] private DialogueOpener _dialogueOpener;

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
            
            bool isSRank = _backlog.IsCurrentSequenceSRank;
            _dialogueManager.IsSRankActive = isSRank;
            if (isSRank)
            {
                MMGameEvent.Trigger("SRankStart");
            }

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
    }
}
