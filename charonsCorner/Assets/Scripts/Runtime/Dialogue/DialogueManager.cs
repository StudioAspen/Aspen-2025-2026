using DG.Tweening;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        [field: SerializeField] public DialogueBacklog Backlog { get; private set; }
        
        [field: SerializeField, ReadOnly] public DialogueOpenerSO CurrentOpener { get; private set; }
        public event Action<DialogueOpenerSO> OnDialogueOpenerStarted = delegate { };

        [field: SerializeField, ReadOnly] public DialogueSequenceSO CurrentSequence { get; private set; }
        public event Action<DialogueSequenceSO> OnDialogueSequenceStarted = delegate { };
        public event Action<DialogueSequenceSO, DialogueSO> OnDialogueSequenceEndReached = delegate { };

        [field: SerializeField, ReadOnly] public int CurrentDialogueIndex { get; private set; }
        [field: SerializeField, ReadOnly] public DialogueSO CurrentDialogue { get; private set; }
        public event Action<DialogueSO> OnDialogueStarted = delegate { };

        public event Action OnDialogueEnded = delegate { };

        public void StartDialogueOpener(DialogueOpenerSO opener)
        {
            if (opener == null)
            {
                Debug.LogWarning("DialogueOpenerSO is null. Cannot start dialogue opener.");
                return;
            }

            CurrentOpener = opener;
            OnDialogueOpenerStarted.Invoke(opener);

            CurrentDialogueIndex = 0;
            CurrentDialogue = opener;
        }

        public void StartDialogueSequence(DialogueSequenceSO sequence)
        {
            if (sequence == null)
            {
                Debug.LogWarning("DialogueSequenceSO is null. Cannot start dialogue sequence.");
                return;
            }

            CurrentSequence = sequence;
            OnDialogueSequenceStarted.Invoke(sequence);

            CurrentDialogueIndex = 0;
            StartDialogue(sequence.DialogueContainers[CurrentDialogueIndex].Dialogue);
        }

        public void StartDialogue(DialogueSO dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("DialogueSO is null. Cannot start dialogue.");
                return;
            }

            CurrentDialogue = dialogue;
            OnDialogueStarted.Invoke(dialogue);
        }

        public void StartNextDialogueInSequence()
        {
            if(CurrentSequence == null)
            {
                Debug.LogWarning("CurrentSequence is null. Cannot start next dialogue in sequence.");
                return;
            }

            if (CurrentDialogueIndex + 1 > CurrentSequence.DialogueContainers.Count)
            {
                Debug.LogWarning("End of dialogue sequence reached. Cannot start next dialogue in sequence.");
                return;
            }

            CurrentDialogueIndex++;
            DialogueSO nextDialogue = CurrentSequence.DialogueContainers[CurrentDialogueIndex].Dialogue;
            StartDialogue(nextDialogue);

            if(CurrentDialogueIndex >= CurrentSequence.DialogueContainers.Count - 1)
                OnDialogueSequenceEndReached.Invoke(CurrentSequence, nextDialogue);
        }

        public void EndDialogue()
        {
            CurrentOpener = null;
            CurrentSequence = null;
            CurrentDialogueIndex = 0;
            CurrentDialogue = null;
            OnDialogueEnded.Invoke();
        }
    }
}
