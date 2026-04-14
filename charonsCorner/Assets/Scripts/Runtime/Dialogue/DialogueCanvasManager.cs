using AYellowpaper.SerializedCollections;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using System.Numerics;
using UnityEngine.SocialPlatforms;

namespace CharonsCorner.Runtime
{
    public class DialogueCanvasManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueManager _dialogueManager;
        [SerializeField] private UIPanel _dialoguePanel;
        [SerializeField] private GameObject _optionButtonPrefab;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TypewriterComponent _dialogueTextTypewriter;
        [SerializeField] private Transform _optionsContainer;

        [Header("Arrow Settings")]
        [SerializeField] private RectTransform _arrowObject;
        [SerializeField] private AnimationCurve _arrowAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _arrowAnimationDuration = 0.5f;
        [SerializeField] private UnityEngine.Vector2 _arrowOffsetX = new UnityEngine.Vector2(-30f, 0f);

        private void Awake()
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
            UIPanel.Focus(_dialoguePanel);
            
            ClearUI();

            _nameText.text = opener.SpeakerName;
            _dialogueTextTypewriter.ShowText(opener.Text);

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
            _dialogueTextTypewriter.ShowText(dialogue.Text);

            ShowNextButton();
        }

        private void DialogueManager_OnDialogueSequenceEndReached(DialogueSequenceSO sequence, DialogueSO dialogue)
        {
            ShowOptions(new(), true);

            ChapterDialogueEntry currentChapterEntry = _dialogueManager.CurrentBacklog.CurrentChapterDialogue;
            if (currentChapterEntry != null)
            {
                if (currentChapterEntry.DialogueOpener.SequenceOptions.Count == 2)
                {
                    int completedIndex =
                        currentChapterEntry.DialogueOpener.SequenceOptions.FindIndex(s => s == sequence);
                    if (completedIndex != -1)
                    {
                        if (FlagManager.Get(_dialogueManager.CurrentBacklog.CurrentDialogueSequenceCompletedFlag) == 0)
                            FlagManager.Set(_dialogueManager.CurrentBacklog.CurrentDialogueSequenceCompletedFlag, completedIndex + 1);
                        else
                            _dialogueManager.CurrentBacklog.CompleteCurrentDialogueSet();
                    }
                }
                else if (currentChapterEntry.DialogueOpener.SequenceOptions.Count == 1)
                {
                    _dialogueManager.CurrentBacklog.CompleteCurrentDialogueSet();
                }
            }
            
            if (_dialogueManager.CurrentBacklog.CurrentSRankDialogue != null)
            {
                if(_dialogueManager.CurrentBacklog.CurrentSRankDialogue.DialogueSequence == sequence)
                    _dialogueManager.CurrentBacklog.CompleteCurrentSRankDialogueSet();
            }
        }
        
        /// <summary>
        /// Stops typing and shows the full text immediately.
        /// </summary>
        public void SkipTyping()
        {
            _dialogueTextTypewriter.SkipTypewriter();
        }

        private void ShowOptions(List<DialogueSequenceSO> sequenceOptions, bool willTryShowReturn = false)
        {
            ClearButtons();

            foreach (DialogueSequenceSO sequence in sequenceOptions)
            {
                GameObject buttonObject = InstantiateOptionButton(_optionsContainer);
                buttonObject.name = $"({sequence.SequenceName})Button";
                TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                buttonText.text = sequence.SequenceName;
                Button button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => _dialogueManager.StartDialogueSequence(sequence));
            }

            if (willTryShowReturn && _dialogueManager.ReturnAction != null)
            {
                GameObject returnButtonObject = InstantiateOptionButton(_optionsContainer);
                returnButtonObject.name = "ReturnButton";
                TMP_Text returnButtonText = returnButtonObject.GetComponentInChildren<TMP_Text>();
                returnButtonText.text = "Return";
                Button returnButton = returnButtonObject.GetComponent<Button>();
                returnButton.onClick.AddListener(() =>
                {
                    _dialogueManager.ReturnAction.Invoke();
                });
            }
            
            GameObject closeButtonObject = InstantiateOptionButton(_optionsContainer);
            closeButtonObject.name = "CloseButton";
            TMP_Text closeButtonText = closeButtonObject.GetComponentInChildren<TMP_Text>();
            closeButtonText.text = "Close";
            Button closeButton = closeButtonObject.GetComponent<Button>();
            closeButton.onClick.AddListener(() => {
                CloseUI();
                _dialogueManager.EndDialogue();
            });

            UIPanel.ChangeCurrentSelectedObject(_optionsContainer.GetChild(0).gameObject); // Set the first button as selected
        }

        private void ShowNextButton()
        {
            ClearButtons();

            GameObject nextButtonObject = InstantiateOptionButton(_optionsContainer);
            nextButtonObject.name = "NextButton";
            TMP_Text nextButtonText = nextButtonObject.GetComponentInChildren<TMP_Text>();
            nextButtonText.text = "Next";

            Button nextButton = nextButtonObject.GetComponent<Button>();
            nextButton.onClick.AddListener(() => _dialogueManager.StartNextDialogueInSequence());

            UIPanel.ChangeCurrentSelectedObject(nextButtonObject);
        }

        private GameObject InstantiateOptionButton(Transform parent)
        {
            GameObject buttonObject = Instantiate(_optionButtonPrefab, parent);
            
            EventTrigger trigger = buttonObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            entry.callback.AddListener((data) => MoveArrowToButton(buttonRect));
            trigger.triggers.Add(entry);

            return buttonObject;
        }

        private void MoveArrowToButton(RectTransform target)
        {
            _arrowObject.gameObject.SetActive(true);
            
            UnityEngine.Vector2 localPosition = _arrowObject.parent.InverseTransformPoint(target.position);
            
            _arrowObject.anchoredPosition = new UnityEngine.Vector2(localPosition.x + _arrowOffsetX.x, localPosition.y);

            DOTween.Kill(_arrowObject);
            _arrowObject.DOAnchorPosY(localPosition.y, _arrowAnimationDuration)
                .SetEase(_arrowAnimationCurve)
                .SetUpdate(true);
        }


        /// <summary>
        /// Completely clears the UI, including name and dialogue text, and removes all option buttons.
        /// Used for resetting the dialogue UI before starting a new dialogue or sequence.
        /// </summary>
        private void ClearUI()
        {
            _nameText.text = "";
            _dialogueTextTypewriter.TextAnimator.SetText("");
            
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

        public void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
            _dialoguePanel.BackOrClose();
        }
    }
}
