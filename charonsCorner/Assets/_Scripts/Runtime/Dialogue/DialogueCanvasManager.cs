using AYellowpaper.SerializedCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using Cysharp.Threading.Tasks;

namespace CharonsCorner.Runtime
{
    public class DialogueCanvasManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private UIPanel _dialoguePanel;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private TMP_Text _inputText;
        [SerializeField] private List<Image> _recoloredImages;
        [SerializeField] private TypewriterComponent _dialogueTextTypewriter;
        [SerializeField] private InputInteraction _inputInteraction;

        [Header("Colors")]
        [SerializeField] private Color _charonColor;
        [SerializeField] private Color _bowleyColor;
        [SerializeField] private Color _mementoColor;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player _dialogueNextFeedback;
        [SerializeField] private MMF_Player _nameBoxChangeFeedback;

        private bool _isTyping;
        private string _lastSpeakerName;
        private bool _isClosing;
        
        private void Awake()
        {
            _dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            _dialogueManager.OnDialogueSequenceStarted += DialogueManager_OnDialogueSequenceStarted;
            _dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            _dialogueManager.OnLineStarted += DialogueManager_OnLineStarted;

            _dialogueManager.OnDialogueSequenceEndReached += DialogueManager_OnDialogueSequenceEndReached;

            _dialogueTextTypewriter.onTextShowed.AddListener(() =>
            {
                _isTyping = false;
                MMGameEvent.Trigger("StopTalk");
                if (_inputInteraction != null) _inputInteraction.Appear();
            });
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
            if (!_dialoguePanel.gameObject.activeInHierarchy || _isClosing)
                return;

            if (_isTyping)
            {
                if (_inputInteraction != null) _inputInteraction.Disappear();
                SkipTyping();
                return;
            }

            if (_inputInteraction != null) _inputInteraction.Disappear();

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
                if (_dialogueNextFeedback != null) _dialogueNextFeedback.PlayFeedbacks();
                _dialogueManager.StartNextDialogueInSequence();
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            _isClosing = false;
            UIPanel.Focus(_dialoguePanel);
            
            ClearUI();
        }

        private void DialogueManager_OnDialogueSequenceStarted(DialogueSequenceSO sequence)
        {
            ClearUI();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            ClearUI(false);

            CheckSpeakerChange(dialogue.SpeakerName);

            _nameText.text = dialogue.SpeakerName;
            _isTyping = true;
            _dialogueTextTypewriter.ShowText(dialogue.Text);
        }

        private void DialogueManager_OnLineStarted(string line)
        {
            ClearUI(false);

            string currentSpeakerName = "";
            // Mapping Speaker enum to string as in DialogueManager.GetProcessedLine
            if (_dialogueManager.CurrentSequence != null && _dialogueManager.CurrentDialogueIndex < _dialogueManager.CurrentSequence.lines.Length)
            {
                Speaker speaker = _dialogueManager.CurrentSequence.lines[_dialogueManager.CurrentDialogueIndex].speaker;
                currentSpeakerName = speaker switch
                {
                    Speaker.Charon => "Charon",
                    Speaker.Bowley => "Bowley",
                    Speaker.Unknown => "???",
                    _ => "???"
                };
            }
            else if (_dialogueManager.CurrentOpener != null)
            {
                currentSpeakerName = _dialogueManager.CurrentOpener.SpeakerName;
            }

            CheckSpeakerChange(currentSpeakerName);

            if (!string.IsNullOrEmpty(currentSpeakerName))
                _nameText.text = currentSpeakerName;
            
            _isTyping = true;
            _dialogueTextTypewriter.ShowText(line);
        }

        private void CheckSpeakerChange(string newSpeakerName)
        {
            if (_lastSpeakerName != newSpeakerName)
            {
                if (_nameBoxChangeFeedback != null) _nameBoxChangeFeedback.PlayFeedbacks();
                UpdateNameBoxColor(newSpeakerName);
            }
            _lastSpeakerName = newSpeakerName;
        }

        private void UpdateNameBoxColor(string speakerName)
        {
            if (_recoloredImages == null || _recoloredImages.Count == 0) return;

            Color targetColor = speakerName switch
            {
                "Charon" => _charonColor,
                "Bowley" => _bowleyColor,
                "???" => _mementoColor,
                _ => _recoloredImages[0].color
            };

            foreach (var image in _recoloredImages)
            {
                if (image != null) image.color = targetColor;
            }

            if (_dialogueText != null) _dialogueText.color = targetColor;
            if (_inputText != null) _inputText.outlineColor = targetColor;
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
            MMGameEvent.Trigger("StopTalk");
        }

        /// <summary>
        /// Completely clears the UI, including name and dialogue text.
        /// Used for resetting the dialogue UI before starting a new dialogue or sequence.
        /// </summary>
        private void ClearUI(bool clearName = true)
        {
            _isTyping = false;
            if (_inputInteraction != null) _inputInteraction.Disappear();
            if (clearName)
            {
                _nameText.text = "";
                _lastSpeakerName = "";
            }
            _dialogueTextTypewriter.TextAnimator.SetText("");
        }

        public void CloseUI()
        {
            CloseUIAsync().Forget();
        }

        private async UniTaskVoid CloseUIAsync()
        {
            if (_isClosing) return;
            _isClosing = true;

            GameManager.Instance.ChangeGameState(GameState.Gameplay);
            _dialoguePanel.BackOrClose();
            _isClosing = false;
        }
    }
}
