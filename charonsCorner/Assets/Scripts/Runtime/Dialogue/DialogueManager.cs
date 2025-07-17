using NaughtyAttributes;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueManager : MonoBehaviour
    {
        [field: SerializeField, ReadOnly] public DialogueSO CurrentDialogue { get; private set; }

        public event Action<DialogueSO> OnDialogueStarted = delegate { };

        /// <summary>
        /// Starts a dialogue with the given DialogueSO.
        /// Can be used to continue a dialogue or start a completely new one.
        /// </summary>
        /// <param name="dialogue"></param>
        public void StartDialogue(DialogueSO dialogue)
        {
            if(dialogue == null)
            {
                Debug.LogError($"DialogueManager: Attempted to start a dialogue with a null DialogueSO. Please check your setup.");
                return;
            }

            if(dialogue == CurrentDialogue)
            {
                Debug.LogWarning($"DialogueManager: Attempted to start the same dialogue '{dialogue.Name}' again. Ignoring.");
                return;
            }

            CurrentDialogue = dialogue;
            OnDialogueStarted.Invoke(dialogue);
        }
    }
}
