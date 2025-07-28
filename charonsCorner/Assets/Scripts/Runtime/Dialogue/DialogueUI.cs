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
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Transform optionsContainer;

        [Header("Typing Config")]
        [SerializeField] private float typingSpeed = 0.01f;
        private Coroutine typingCoroutine;
        private string typedOutFullText;

        private protected override void Initialize()
        {
            dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            dialogueManager.OnDialogueSequenceStarted += DialogueManager_OnDialogueSequenceStarted;
            dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;

            dialogueManager.OnDialogueSequenceEndReached += DialogueManager_OnDialogueSequenceEndReached;
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueOpenerStarted -= DialogueManager_OnDialogueOpenerStarted;
                dialogueManager.OnDialogueSequenceStarted -= DialogueManager_OnDialogueSequenceStarted;
                dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;

                dialogueManager.OnDialogueSequenceEndReached -= DialogueManager_OnDialogueSequenceEndReached;
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            ClearUI();

            nameText.text = opener.SpeakerName;
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

            nameText.text = dialogue.SpeakerName;
            TypeOutText(dialogue.Text);

            ShowNextButton();
        }

        private void DialogueManager_OnDialogueSequenceEndReached(DialogueSequenceSO sequence, DialogueSO dialogue)
        {
            ShowOptions(new());
        }

        private void TypeOutText(string text)
        {
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typedOutFullText = text;
            typingCoroutine = StartCoroutine(TypeOutTextCoroutine(text));
        }

        /// <summary>
        /// Types out the dialogue text character by character with a specified delay.
        /// </summary>
        private IEnumerator TypeOutTextCoroutine(string text)
        {
            dialogueText.text = "";

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            typingCoroutine = null;
        }

        /// <summary>
        /// Stops typing and shows the full text immediately.
        /// </summary>
        public void SkipTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;

                dialogueText.text = typedOutFullText;
            }
        }

        private void ShowOptions(List<DialogueSequenceSO> sequenceOptions)
        {
            ClearButtons();

            foreach (DialogueSequenceSO sequence in sequenceOptions)
            {
                GameObject buttonObject = Instantiate(optionButtonPrefab, optionsContainer);
                buttonObject.name = $"({sequence.SequenceName})Button";
                TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                buttonText.text = sequence.SequenceName;
                Button button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => dialogueManager.StartDialogueSequence(sequence));
            }

            GameObject closeButtonObject = Instantiate(optionButtonPrefab, optionsContainer);
            closeButtonObject.name = "CloseButton";
            TMP_Text closeButtonText = closeButtonObject.GetComponentInChildren<TMP_Text>();
            closeButtonText.text = "Close";
            Button closeButton = closeButtonObject.GetComponent<Button>();
            closeButton.onClick.AddListener(() => {
                CloseUI();
                dialogueManager.EndDialogue();
            });

            if (optionsContainer.childCount > 0)
                uiManager.ChangeCurrentSelectedObject(optionsContainer.GetChild(0).gameObject);
        }

        private void ShowNextButton()
        {
            ClearButtons();

            GameObject nextButtonObject = Instantiate(optionButtonPrefab, optionsContainer);
            nextButtonObject.name = "NextButton";
            TMP_Text nextButtonText = nextButtonObject.GetComponentInChildren<TMP_Text>();
            nextButtonText.text = "Next";

            Button nextButton = nextButtonObject.GetComponent<Button>();
            nextButton.onClick.AddListener(() => dialogueManager.StartNextDialogueInSequence());
        }

        /// <summary>
        /// Completely clears the UI, including name and dialogue text, and removes all option buttons.
        /// Used for resetting the dialogue UI before starting a new dialogue or sequence.
        /// </summary>
        private void ClearUI()
        {
            nameText.text = "";
            dialogueText.text = "";

            ClearButtons();
        }

        private void ClearButtons()
        {
            foreach (Transform child in optionsContainer)
                Destroy(child.gameObject);
        }

        public override void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
