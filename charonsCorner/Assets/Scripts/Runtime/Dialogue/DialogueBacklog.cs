using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueBacklog : MonoBehaviour
    {
        [field: SerializeField] public List<ChapterDialogueEntry> ChapterDialogues { get; private set; } = new();
        
        /// <summary>
        /// Tracks the current opener.
        /// </summary>
        public ChapterDialogueEntry CurrentChapterDialogue { get; private set; }

        public DialogueOpenerSO GetCurrentDialogueOpener()
        {
            if (!HasPendingDialogue())
                return null;

            int currDialogueOpenerIndex = FlagManager.Get(ProgressFlag.CurrentDialogueOpenerIndex);
            ChapterDialogueEntry entry =
                ChapterDialogues.Find(e => e.ChapterIndex == currDialogueOpenerIndex);

            if (entry == null)
            {
                Debug.LogWarning($"No dialogue entry found for ChapterIndex {currDialogueOpenerIndex}");
                return null;
            }

            CurrentChapterDialogue = entry;
            
            // Remove an already completed option
            DialogueOpenerSO runtimeOpener = Instantiate(entry.DialogueOpener); // Instantiate a runtime copy so we never mutate the asset
            
            int completedSequenceIndex = FlagManager.Get(ProgressFlag.CurrentDialogueSequenceCompleted);
            if (runtimeOpener.SequenceOptions == null || runtimeOpener.SequenceOptions.Count == 0)
                return runtimeOpener;

            if (completedSequenceIndex == 1)
                runtimeOpener.SetSequenceOptions(new List<DialogueSequenceSO>(){runtimeOpener.SequenceOptions[1]});
            else if (completedSequenceIndex == 2)
                runtimeOpener.SetSequenceOptions(new List<DialogueSequenceSO>(){runtimeOpener.SequenceOptions[0]});
            
            return runtimeOpener;
        }

        /// <summary>
        /// Marks the current chapter's dialogue as completed
        /// and advances the dialogue index.
        /// </summary>
        public void CompleteCurrentDialogueSet()
        {
            FlagManager.Increment(ProgressFlag.CurrentDialogueOpenerIndex);
            FlagManager.Set(ProgressFlag.CurrentDialogueSequenceCompleted, 0);
            CurrentChapterDialogue = null;
        }

        /// <summary>
        /// Returns true if there is pending dialogue
        /// that Charon has not yet delivered.
        /// </summary>
        public bool HasPendingDialogue()
        {
            return FlagManager.Get(ProgressFlag.CurrentDialogueOpenerIndex)
                   <= FlagManager.Get(ProgressFlag.CurrentChapterIndex);
        }
    }
}