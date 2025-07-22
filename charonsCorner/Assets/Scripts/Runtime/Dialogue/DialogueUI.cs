using AYellowpaper.SerializedCollections;
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
        [SerializeField] private DialogueConfigSO dialogueConfig;
        [SerializeField] private GameObject optionButtonPrefab;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private Image reactionImage;
        [SerializeField] private Transform optionsContainer;

        [Header("Typing Config")]
        [SerializeField] private float typingSpeed = 0.01f;
        private Coroutine typingCoroutine;
        private string typedOutFullText;
        private Dictionary<string, DialogueSO> pendingOptions;

        private protected override void Initialize()
        {
            dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
                dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            ClearUI();

            if (dialogue == null)
                return;

            nameText.text = dialogue.Name;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typedOutFullText = dialogue.Text;
            pendingOptions = dialogue.Options;

            typingCoroutine = StartCoroutine(TypeOutTextCoroutine(typedOutFullText));

            if (dialogueConfig.ReactionSprites.TryGetValue(dialogue.Reaction, out Sprite reactionSprite))
                reactionImage.sprite = reactionSprite;
            else
                reactionImage.sprite = null;
        }

        /// <summary>
        /// Types out the dialogue text character by character with a specified delay.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private IEnumerator TypeOutTextCoroutine(string text)
        {
            dialogueText.text = "";

            foreach (char letter in text)
            {
                dialogueText.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            typingCoroutine = null;
            ShowOptions(pendingOptions);
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
                ShowOptions(pendingOptions);
            }
        }

        /// <summary>
        /// Displays the options available.
        /// </summary>
        /// <param name="options"></param>
        private void ShowOptions(Dictionary<string, DialogueSO> options)
        {
            if(options == null || options.Count == 0)
            {
                GameObject closeButtonObject = Instantiate(optionButtonPrefab, optionsContainer);
                closeButtonObject.name = "CloseButton";
                TMP_Text closeButtonText = closeButtonObject.GetComponentInChildren<TMP_Text>();
                closeButtonText.text = "Close";

                Button closeButton = closeButtonObject.GetComponent<Button>();
                closeButton.onClick.AddListener(CloseUI);
                return;
            }

            foreach (var option in options)
            {
                GameObject buttonObject = Instantiate(optionButtonPrefab, optionsContainer);
                buttonObject.name = $"({option.Key})Button";
                TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                buttonText.text = option.Key;

                Button button = buttonObject.GetComponent<Button>();
                DialogueSO nextDialogue = option.Value;
                button.onClick.AddListener(() => dialogueManager.StartDialogue(nextDialogue));
            }

            if(optionsContainer.childCount > 0)
                uiManager.ChangeCurrentSelectedObject(optionsContainer.GetChild(0).gameObject);
        }

        /// <summary>
        /// Completely clears the UI elements to prepare for a new dialogue to be typed out.
        /// </summary>
        private void ClearUI()
        {
            nameText.text = "";
            dialogueText.text = "";
            reactionImage.sprite = null;

            foreach (Transform child in optionsContainer)
                Destroy(child.gameObject);
        }

        public override void CloseUI()
        {
            // No specific close behavior for dialogue UI
            Debug.LogWarning("DialogueUI: CloseUI called, but no specific close behavior defined.");
        }
    }
}
