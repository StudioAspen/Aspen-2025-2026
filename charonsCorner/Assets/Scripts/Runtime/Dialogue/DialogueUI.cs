using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class DialogueUI : UIPanel
    {
        [Header("References")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private GameObject _optionButtonPrefab;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _dialogueText;
        [SerializeField] private Transform _optionsContainer;

        [Header("Typing Config")]
        [SerializeField] private float _typingSpeed = 0.01f;
        private Coroutine _typingCoroutine;
        private string _typedOutFullText;

        private protected override void Initialize()
        {
            _dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            _dialogueManager.OnDialogueSequenceStarted += DialogueManager_OnDialogueSequenceStarted;
            _dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;

            _dialogueManager.OnDialogueSequenceEndReached += DialogueManager_OnDialogueSequenceEndReached;
        }

        private void OnDestroy()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.OnDialogueOpenerStarted -= DialogueManager_OnDialogueOpenerStarted;
                _dialogueManager.OnDialogueSequenceStarted -= DialogueManager_OnDialogueSequenceStarted;
                _dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;

                _dialogueManager.OnDialogueSequenceEndReached -= DialogueManager_OnDialogueSequenceEndReached;
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            ClearUI();

            _nameText.text = opener.SpeakerName;
            TypeOutText(opener.Text);

            ShowOptions(opener.SequenceOptions);
        }

        private void DialogueManager_OnDialogueSequenceStarted(DialogueSequenceSO sequence)
        {
            ClearUI();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            ClearUI();

            _nameText.text = dialogue.SpeakerName;
            TypeOutText(dialogue.Text);

            ShowNextButton();
        }

        private void DialogueManager_OnDialogueSequenceEndReached(DialogueSequenceSO sequence, DialogueSO dialogue)
        {
            ShowOptions(new());
        }

        private void TypeOutText(string text)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);
            _typedOutFullText = text;
            _typingCoroutine = StartCoroutine(TypeOutTextCoroutine(text));
        }

        /// <summary>
        /// Types out the dialogue text character by character with a specified delay.
        /// </summary>
        private IEnumerator TypeOutTextCoroutine(string text)
        {
            _dialogueText.text = "";

            foreach (char letter in text)
            {
                _dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(_typingSpeed);
            }

            _typingCoroutine = null;
        }

        /// <summary>
        /// Stops typing and shows the full text immediately.
        /// </summary>
        public void SkipTyping()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;

                _dialogueText.text = _typedOutFullText;
            }
        }

        private void ShowOptions(List<DialogueSequenceSO> sequenceOptions)
        {
            ClearButtons();

            foreach (DialogueSequenceSO sequence in sequenceOptions)
            {
                GameObject buttonObject = Instantiate(_optionButtonPrefab, _optionsContainer);
                buttonObject.name = $"({sequence.SequenceName})Button";
                TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                buttonText.text = sequence.SequenceName;
                Button button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => _dialogueManager.StartDialogueSequence(sequence));
            }

            GameObject closeButtonObject = Instantiate(_optionButtonPrefab, _optionsContainer);
            closeButtonObject.name = "CloseButton";
            TMP_Text closeButtonText = closeButtonObject.GetComponentInChildren<TMP_Text>();
            closeButtonText.text = "Close";
            Button closeButton = closeButtonObject.GetComponent<Button>();
            closeButton.onClick.AddListener(() => {
                CloseUI();
                _dialogueManager.EndDialogue();
            });

            _uiManager.ChangeCurrentSelectedObject(_optionsContainer.GetChild(0).gameObject); // Set the first button as selected
        }

        private void ShowNextButton()
        {
            ClearButtons();

            GameObject nextButtonObject = Instantiate(_optionButtonPrefab, _optionsContainer);
            nextButtonObject.name = "NextButton";
            TMP_Text nextButtonText = nextButtonObject.GetComponentInChildren<TMP_Text>();
            nextButtonText.text = "Next";

            Button nextButton = nextButtonObject.GetComponent<Button>();
            nextButton.onClick.AddListener(() => _dialogueManager.StartNextDialogueInSequence());

            _uiManager.ChangeCurrentSelectedObject(nextButtonObject);
        }

        /// <summary>
        /// Completely clears the UI, including name and dialogue text, and removes all option buttons.
        /// Used for resetting the dialogue UI before starting a new dialogue or sequence.
        /// </summary>
        private void ClearUI()
        {
            _nameText.text = "";
            _dialogueText.text = "";

            ClearButtons();
        }

        private void ClearButtons()
        {
            // We need to detach first because destroyed gameObjects take a frame
            // We are looping backwards to avoid issues with child count changing during iteration
            for (int i = _optionsContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = _optionsContainer.GetChild(i);
                child.SetParent(null, false); // Detach so container's childCount updates immediately
                Destroy(child.gameObject);
            }
        }

        public override void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
