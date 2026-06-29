using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.TextAnimatorForUnity.TextMeshPro;

namespace CharonsCorner.Runtime
{
    public class DialogueCanvasManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private UIPanel _dialoguePanel;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TypewriterComponent _dialogueTextTypewriter;

        private bool _isTyping;
        
        private void Awake()
        {
            _dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            _dialogueManager.OnDialogueSequenceStarted += DialogueManager_OnDialogueSequenceStarted;
            _dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            _dialogueManager.OnLineStarted += DialogueManager_OnLineStarted;

            _dialogueManager.OnDialogueSequenceEndReached += DialogueManager_OnDialogueSequenceEndReached;

            _dialogueTextTypewriter.onTextShowed.AddListener(() => _isTyping = false);
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.Interact += HandleInteract;
            }
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.Interact -= HandleInteract;
            }
        }

        private void OnDestroy()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.OnDialogueOpenerStarted -= DialogueManager_OnDialogueOpenerStarted;
                _dialogueManager.OnDialogueSequenceStarted -= DialogueManager_OnDialogueSequenceStarted;
                _dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;
                _dialogueManager.OnLineStarted -= DialogueManager_OnLineStarted;

                _dialogueManager.OnDialogueSequenceEndReached -= DialogueManager_OnDialogueSequenceEndReached;
            }
        }

        private void HandleInteract()
        {
            if (!_dialoguePanel.gameObject.activeInHierarchy)
                return;

            if (_isTyping)
            {
                SkipTyping();
                return;
            }

            if (_dialogueManager.CurrentSequence == null)
            {
                // Since we start the first sequence immediately in StartDialogueOpener,
                // we should ideally never be here if the opener has sequences.
                // But if for some reason we are, we just close or handle it.
                CloseUI();
                _dialogueManager.EndDialogue();
            }
            else if (_dialogueManager.CurrentDialogueIndex + 1 >= _dialogueManager.CurrentSequence.lines.Length)
            {
                CloseUI();
                _dialogueManager.EndDialogue();
            }
            else
            {
                _dialogueManager.StartNextDialogueInSequence();
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            UIPanel.Focus(_dialoguePanel);
            
            ClearUI();
        }

        private void DialogueManager_OnDialogueSequenceStarted(DialogueSequenceSO sequence)
        {
            ClearUI();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            ClearUI();

            _nameText.text = dialogue.SpeakerName;
            _isTyping = true;
            _dialogueTextTypewriter.ShowText(dialogue.Text);
        }

        private void DialogueManager_OnLineStarted(string line)
        {
            ClearUI();

            // Keep the name of the opener speaker if we are in a sequence
            if (_dialogueManager.CurrentOpener != null)
                _nameText.text = _dialogueManager.CurrentOpener.SpeakerName;
            
            _isTyping = true;
            _dialogueTextTypewriter.ShowText(line);
        }

        private void DialogueManager_OnDialogueSequenceEndReached(DialogueSequenceSO sequence, string line)
        {
            _dialogueManager.CurrentBacklog.CompleteCurrentSequence();
        }
        
        /// <summary>
        /// Stops typing and shows the full text immediately.
        /// </summary>
        public void SkipTyping()
        {
            _isTyping = false;
            _dialogueTextTypewriter.SkipTypewriter();
        }

        /// <summary>
        /// Completely clears the UI, including name and dialogue text.
        /// Used for resetting the dialogue UI before starting a new dialogue or sequence.
        /// </summary>
        private void ClearUI()
        {
            _isTyping = false;
            _nameText.text = "";
            _dialogueTextTypewriter.TextAnimator.SetText("");
        }

        public void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
            _dialoguePanel.BackOrClose();
        }
    }
}
