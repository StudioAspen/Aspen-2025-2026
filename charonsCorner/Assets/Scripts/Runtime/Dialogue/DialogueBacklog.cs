using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueBacklog : MonoBehaviour
    {
        [field: SerializeField] public List<ChapterDialogueEntry> ChapterDialogues { get; private set; } = new();
        [field: SerializeField] public List<ChapterSRankDialogueEntry> SRankDialogues { get; private set; } = new();
        [field: SerializeField] public DialogueOpenerSO DefaultCharonDialogueOpener { get; private set; }
        
        /// <summary>
        /// Tracks the current opener.
        /// </summary>
        public ChapterDialogueEntry CurrentChapterDialogue { get; private set; }
        public ChapterSRankDialogueEntry CurrentSRankDialogue { get; private set; }

        public DialogueOpenerSO GetCurrentDialogueOpener()
        {
            if (!HasPendingDialogue())
                return null;

            ChapterDialogueEntry entry = null;
            int currDialogueOpenerIndex = FlagManager.Get(ProgressFlag.CurrentDialogueOpenerIndex);
            if (HasPendingRegularDialogue())
            {
                entry =
                    ChapterDialogues.Find(e => e.ChapterIndex == currDialogueOpenerIndex);
                if (entry == null)
                    Debug.LogWarning($"No dialogue entry found for ChapterIndex {currDialogueOpenerIndex}");
            }
            
            ChapterSRankDialogueEntry sRankEntry = null;
            if (HasPendingSRankDialogue())
            {
                int currSRankDialogueSequenceIndex = FlagManager.Get(ProgressFlag.CurrentSRankDialogueIndex);
                sRankEntry =
                    SRankDialogues.Find(e => e.ChapterIndex == currSRankDialogueSequenceIndex);
                if(sRankEntry == null)
                    Debug.LogWarning($"No S rank entry found for ChapterIndex {currDialogueOpenerIndex}");
            }
            
            CurrentChapterDialogue = entry;
            CurrentSRankDialogue = sRankEntry;
            
            DialogueOpenerSO runtimeOpener;
            if (entry != null)
                runtimeOpener = Instantiate(entry.DialogueOpener);
            else
                runtimeOpener = Instantiate(DefaultCharonDialogueOpener);
            
            // Add regular sequences first
            if (entry != null)
            {
                int completedSequenceIndex = FlagManager.Get(ProgressFlag.CurrentDialogueSequenceCompleted);
                if (completedSequenceIndex == 1)
                    runtimeOpener.SetSequenceOptions(new List<DialogueSequenceSO>(){entry.DialogueOpener.SequenceOptions[1]});
                else if (completedSequenceIndex == 2)
                    runtimeOpener.SetSequenceOptions(new List<DialogueSequenceSO>(){entry.DialogueOpener.SequenceOptions[0]});
            }
            
            // Add s rank sequence after
            if (sRankEntry != null)
            {
                List<DialogueSequenceSO> mergedList = new List<DialogueSequenceSO>(runtimeOpener.SequenceOptions);
                mergedList.Add(sRankEntry.DialogueSequence);
                runtimeOpener.SetSequenceOptions(mergedList);
            }
            
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

        public void CompleteCurrentSRankDialogueSet()
        {
            FlagManager.Increment(ProgressFlag.CurrentSRankDialogueIndex);
            CurrentSRankDialogue = null;
        }

        /// <summary>
        /// Returns true if there is pending dialogue
        /// that Charon has not yet delivered.
        /// </summary>
        public bool HasPendingDialogue()
        {
            return HasPendingRegularDialogue() || HasPendingSRankDialogue();
        }

        public bool HasPendingRegularDialogue()
        {
            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            bool hasRegularDialogue = FlagManager.Get(ProgressFlag.CurrentDialogueOpenerIndex)
                                      <= currentChapterIndex;

            return hasRegularDialogue;
        }

        public bool HasPendingSRankDialogue()
        {
            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            var sRanksAchievedList = SaveManager.GameStore.GetList<int>(DialogueSaveKeys.SRankAchievedListKey, new());
            bool hasSRankForCurrChapter = sRanksAchievedList.Contains(currentChapterIndex);
            bool hasSRankDialogue = (FlagManager.Get(ProgressFlag.CurrentSRankDialogueIndex) <= currentChapterIndex)
                                    && hasSRankForCurrChapter;

            return hasSRankDialogue;
        }

        [Button("Force S Rank On Chapter")]
        public void ForceSRankOnChapter(int chapterIndex)
        {
            var list = SaveManager.GameStore.GetList<int>(DialogueSaveKeys.SRankAchievedListKey, new());
            if (!list.Contains(chapterIndex))
            {
                list.Add(chapterIndex);
                SaveManager.GameStore.SetList(DialogueSaveKeys.SRankAchievedListKey, list);
            }
        }

        [Button("Clear S Ranks On All Chapter")]
        public void ClearSRanksOnAllChapters()
        {
            SaveManager.GameStore.SetList(DialogueSaveKeys.SRankAchievedListKey, new List<int>());
        }
    }
}