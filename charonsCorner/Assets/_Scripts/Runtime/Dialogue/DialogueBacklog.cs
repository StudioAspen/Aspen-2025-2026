using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueBacklog : MonoBehaviour
    {
        [field: SerializeField] public List<ChapterDialogueEntry> ChapterDialogues { get; private set; } = new();
        [field: SerializeField] public List<ChapterSRankDialogueEntry> SRankDialogues { get; private set; } = new();
        [field: SerializeField] public DialogueOpenerSO DefaultCharonDialogueOpener { get; private set; }
        
        [Header("Override Settings (Testing Only)")]
        [SerializeField] private bool _overrideSaveData;
        [SerializeField, ReadOnly, ShowIf(nameof(_overrideSaveData))] private ChapterDialogueEntry _overriddenDialogueEntry;
        [SerializeField, ReadOnly, ShowIf(nameof(_overrideSaveData))] private ChapterSRankDialogueEntry _overriddenSRankEntry;

        public void SetOverriddenDialogue(ChapterDialogueEntry entry)
        {
            if (!_overrideSaveData)
            {
                Debug.LogWarning("Cannot override dialogue because OverrideSaveData is not active.");
                return;
            }
            _overriddenDialogueEntry = entry;
            _overriddenSRankEntry = null;
            
            // Reset sequence index when forcing a new dialogue
            FlagManager.Set(CurrentDialogueSequenceIndexFlag, 0);
            
            Debug.Log($"[DialogueBacklog] Overriding with Dialogue Entry for Chapter {entry.ChapterIndex}");
        }

        public void SetOverriddenSRankDialogue(ChapterSRankDialogueEntry entry)
        {
            if (!_overrideSaveData)
            {
                Debug.LogWarning("Cannot override dialogue because OverrideSaveData is not active.");
                return;
            }
            _overriddenSRankEntry = entry;
            _overriddenDialogueEntry = null;
            Debug.Log($"[DialogueBacklog] Overriding with S-Rank Dialogue for Chapter {entry.ChapterIndex}");
        }

        [Header("Alert Feedbacks")]
        [SerializeField] private MMF_Player _alertEnterFeedback;
        [SerializeField] private MMF_Player _alertExitFeedback;

        /// <summary>
        /// Tracks the current opener.
        /// </summary>
        public ChapterDialogueEntry CurrentChapterDialogue { get; private set; }
        public ChapterSRankDialogueEntry CurrentSRankDialogue { get; private set; }
        public bool IsCurrentSequenceSRank { get; private set; }
        
        [field: SerializeField] public ProgressFlag CurrentDialogueOpenerIndexFlag { get; private set; } = ProgressFlag.CurrentDialogueOpenerIndex;
        [field: SerializeField] public ProgressFlag CurrentDialogueSequenceIndexFlag { get; private set; } = ProgressFlag.CurrentDialogueSequenceIndex;

        [Header("Debug")]
        [SerializeField] private bool _showDebug = false;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += GameManager_OnGameStateChanged;
                GameManager.Instance.OnBeforeGameStateChanged += GameManager_OnBeforeGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= GameManager_OnGameStateChanged;
                GameManager.Instance.OnBeforeGameStateChanged -= GameManager_OnBeforeGameStateChanged;
            }
        }

        private void GameManager_OnBeforeGameStateChanged(GameState currentState, GameState newState)
        {
            if (currentState == GameState.Gameplay && newState != GameState.Gameplay)
            {
                if (_alertExitFeedback != null)
                {
                    _alertExitFeedback.PlayFeedbacks();
                }
            }
        }

        private void GameManager_OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.Gameplay)
            {
                if (HasPendingDialogue())
                {
                    if (_alertEnterFeedback != null)
                    {
                        _alertEnterFeedback.PlayFeedbacks();
                    }
                }
            }
        }

        public DialogueOpenerSO GetCurrentDialogueOpener()
        {
            DialogueOpenerSO runtimeOpener;
            if (_showDebug)
            {
                int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
                int currDialogueOpenerIndex = FlagManager.Get(CurrentDialogueOpenerIndexFlag);
                int sequenceIndex = FlagManager.Get(CurrentDialogueSequenceIndexFlag);
                int currSRankIndex = FlagManager.Get(ProgressFlag.CurrentSRankDialogueIndex);

                Debug.Log($"[DialogueBacklog] Getting dialogue opener. " +
                          $"CurrentChapterIndex: {currentChapterIndex}, " +
                          $"CurrentDialogueOpenerIndex: {currDialogueOpenerIndex}, " +
                          $"CurrentDialogueSequenceIndex: {sequenceIndex}, " +
                          $"CurrentSRankDialogueIndex: {currSRankIndex}, " +
                          $"HasPendingRegular: {HasPendingRegularDialogue()}, " +
                          $"HasPendingSRank: {HasPendingSRankDialogue()}");
            }

            if (_overrideSaveData)
            {
                if (_overriddenDialogueEntry != null)
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] Using Overridden Dialogue Entry for Chapter {_overriddenDialogueEntry.ChapterIndex}");
                    CurrentChapterDialogue = _overriddenDialogueEntry;
                    CurrentSRankDialogue = null;
                    IsCurrentSequenceSRank = false;
                    
                    if (_overriddenDialogueEntry.DialogueOpener == null)
                    {
                        Debug.LogError("[DialogueBacklog] OverriddenDialogueEntry.DialogueOpener is null!");
                        return null;
                    }
                    runtimeOpener = Instantiate(_overriddenDialogueEntry.DialogueOpener);
                    
                    List<DialogueSequenceSO> overriddenSequences = new List<DialogueSequenceSO>();
                    int sequenceIndex = FlagManager.Get(CurrentDialogueSequenceIndexFlag);
                    
                    if (sequenceIndex < _overriddenDialogueEntry.DialogueOpener.SequenceOptions.Count)
                    {
                        if (_showDebug) Debug.Log($"[DialogueBacklog] Selecting sequence index {sequenceIndex} from overridden opener.");
                        overriddenSequences.Add(_overriddenDialogueEntry.DialogueOpener.SequenceOptions[sequenceIndex]);
                    }
                    else if (_overriddenDialogueEntry.DialogueOpener.SequenceOptions.Count > 0)
                    {
                        if (_showDebug) Debug.Log($"[DialogueBacklog] Sequence index {sequenceIndex} out of bounds for overridden opener. Selecting exhausted sequence.");
                        // Play exhausted sequence (last one) if all sequences are done
                        overriddenSequences.Add(_overriddenDialogueEntry.DialogueOpener.SequenceOptions[_overriddenDialogueEntry.DialogueOpener.SequenceOptions.Count - 1]);
                    }

                    runtimeOpener.SetSequenceOptions(overriddenSequences);
                    return runtimeOpener;
                }
                
                if (_overriddenSRankEntry != null)
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] Using Overridden S-Rank Entry for Chapter {_overriddenSRankEntry.ChapterIndex}");
                    CurrentChapterDialogue = null;
                    CurrentSRankDialogue = _overriddenSRankEntry;
                    IsCurrentSequenceSRank = true;

                    if (DefaultCharonDialogueOpener == null)
                    {
                        Debug.LogError("[DialogueBacklog] DefaultCharonDialogueOpener is null! Cannot instantiate it.");
                        return null;
                    }

                    runtimeOpener = Instantiate(DefaultCharonDialogueOpener);
                    runtimeOpener.SetSequenceOptions(new List<DialogueSequenceSO> { _overriddenSRankEntry.DialogueSequence });
                    return runtimeOpener;
                }
            }

            ChapterDialogueEntry entry = null;
            int currentOpenerIndex = FlagManager.Get(CurrentDialogueOpenerIndexFlag);
            int globalChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            
            if (HasPendingRegularDialogue())
            {
                // Find the first entry that is within the range [currentOpenerIndex, globalChapterIndex]
                entry = ChapterDialogues
                    .FindAll(e => e.ChapterIndex >= currentOpenerIndex && e.ChapterIndex <= globalChapterIndex)
                    .Find(e => true); // Get the first one in the list order that matches criteria
                
                if (entry == null)
                    Debug.LogWarning($"No dialogue entry found for ChapterIndex >= {currentOpenerIndex}");
                else if (entry.ChapterIndex > currentOpenerIndex)
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] Skipping to next available chapter dialogue: {entry.ChapterIndex}");
                    FlagManager.Set(CurrentDialogueOpenerIndexFlag, entry.ChapterIndex);
                    currentOpenerIndex = entry.ChapterIndex;
                }
            }
            
            ChapterSRankDialogueEntry sRankEntry = null;
            if (HasPendingSRankDialogue())
            {
                int currSRankDialogueIndex = FlagManager.Get(ProgressFlag.CurrentSRankDialogueIndex);
                if (currSRankDialogueIndex >= 0 && currSRankDialogueIndex < SRankDialogues.Count)
                {
                    sRankEntry = SRankDialogues[currSRankDialogueIndex];
                }
                
                if(sRankEntry == null)
                    Debug.LogWarning($"No S rank entry found for index {currSRankDialogueIndex}");
            }
            
            CurrentChapterDialogue = entry;
            CurrentSRankDialogue = sRankEntry;
            
            if (entry != null)
            {
                if (_showDebug) Debug.Log($"[DialogueBacklog] Found regular dialogue entry for Chapter {entry.ChapterIndex}. Selecting its opener: {entry.DialogueOpener.name}");
                if (entry.DialogueOpener == null)
                {
                    Debug.LogError($"[DialogueBacklog] DialogueOpener is null for chapter {entry.ChapterIndex}!");
                    return null;
                }
                runtimeOpener = Instantiate(entry.DialogueOpener);
            }
            else if (ChapterDialogues.Count > 0)
            {
                // If we finished all chapter dialogues, we use the most recent one's exhausted sequence
                ChapterDialogueEntry lastEntry = ChapterDialogues
                    .FindAll(e => e.ChapterIndex <= globalChapterIndex)
                    .FindLast(e => true);

                if (lastEntry != null)
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] No pending regular dialogue. Using fallback from Chapter {lastEntry.ChapterIndex}.");
                    if (lastEntry.DialogueOpener == null)
                    {
                        Debug.LogError($"[DialogueBacklog] DialogueOpener is null for chapter {lastEntry.ChapterIndex} (fallback)!");
                        return null;
                    }
                    runtimeOpener = Instantiate(lastEntry.DialogueOpener);
                }
                else
                {
                    if (_showDebug) Debug.Log("[DialogueBacklog] No past chapter dialogues available. Using DefaultCharonDialogueOpener.");
                    runtimeOpener = Instantiate(DefaultCharonDialogueOpener);
                }
            }
            else
            {
                if (_showDebug) Debug.Log("[DialogueBacklog] No chapter dialogues available. Using DefaultCharonDialogueOpener.");
                if (DefaultCharonDialogueOpener == null)
                {
                    Debug.LogError("[DialogueBacklog] DefaultCharonDialogueOpener is null (fallback)!");
                    return null;
                }
                runtimeOpener = Instantiate(DefaultCharonDialogueOpener);
            }

            List<DialogueSequenceSO> sequencesToPlay = new List<DialogueSequenceSO>();

            // 1. Regular Sequences for the current chapter
            if (entry != null)
            {
                int sequenceIndex = FlagManager.Get(CurrentDialogueSequenceIndexFlag);
                if (sequenceIndex < entry.DialogueOpener.SequenceOptions.Count)
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] Adding regular sequence at index {sequenceIndex}: {entry.DialogueOpener.SequenceOptions[sequenceIndex].name}");
                    sequencesToPlay.Add(entry.DialogueOpener.SequenceOptions[sequenceIndex]);
                }
                else
                {
                    if (_showDebug) Debug.Log($"[DialogueBacklog] Regular sequences for Chapter {entry.ChapterIndex} are exhausted (Index: {sequenceIndex}).");
                }
            }

            // 2. S-Rank Sequences (only if regular sequences for the current chapter are done)
            if (sRankEntry != null && sequencesToPlay.Count == 0)
            {
                if (_showDebug) Debug.Log($"[DialogueBacklog] Adding S-Rank sequence: {sRankEntry.DialogueSequence.name}");
                sequencesToPlay.Add(sRankEntry.DialogueSequence);
                IsCurrentSequenceSRank = true;
            }
            else
            {
                IsCurrentSequenceSRank = false;
            }

            // 3. Exhausted Sequence (if no new regular or S-rank sequences are available)
            if (sequencesToPlay.Count == 0)
            {
                if (runtimeOpener.SequenceOptions.Count > 0)
                {
                    var exhaustedSequence = runtimeOpener.SequenceOptions[runtimeOpener.SequenceOptions.Count - 1];
                    if (_showDebug) Debug.Log($"[DialogueBacklog] No new sequences available. Playing exhausted sequence: {exhaustedSequence.name}");
                    sequencesToPlay.Add(exhaustedSequence);
                }
            }

            runtimeOpener.SetSequenceOptions(sequencesToPlay);
            return runtimeOpener;
        }

        /// <summary>
        /// Marks the current sequence as completed.
        /// If all sequences in the opener are done, advances to the next opener.
        /// </summary>
        public void CompleteCurrentSequence()
        {
            if (CurrentChapterDialogue != null)
            {
                int sequenceIndex = FlagManager.Get(CurrentDialogueSequenceIndexFlag);
                sequenceIndex++;
                
                if (sequenceIndex >= CurrentChapterDialogue.DialogueOpener.SequenceOptions.Count)
                {
                    CompleteCurrentDialogueSet();
                }
                else
                {
                    FlagManager.Set(CurrentDialogueSequenceIndexFlag, sequenceIndex);
                }
            }
            else if (CurrentSRankDialogue != null)
            {
                CompleteCurrentSRankDialogueSet();
            }
        }

        public void CompleteCurrentDialogueSet()
        {
            int currentOpenerIndex = FlagManager.Get(CurrentDialogueOpenerIndexFlag);
            int globalChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);

            if (currentOpenerIndex < globalChapterIndex)
            {
                FlagManager.Increment(CurrentDialogueOpenerIndexFlag);
            }
            
            FlagManager.Set(CurrentDialogueSequenceIndexFlag, 0);
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
            if (_overrideSaveData && (_overriddenDialogueEntry != null || _overriddenSRankEntry != null))
                return true;
            
            return HasPendingRegularDialogue() || HasPendingSRankDialogue();
        }

        public bool HasPendingRegularDialogue()
        {
            if (_overrideSaveData)
                return _overriddenDialogueEntry != null;

            int currentDialogueOpenerIndex = FlagManager.Get(CurrentDialogueOpenerIndexFlag);
            
            // We have pending dialogue if there's any entry in the list whose ChapterIndex is 
            // greater than or equal to the CurrentDialogueOpenerIndex, 
            // AND that ChapterIndex is less than or equal to the current overall progress.
            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);

            bool hasRegularDialogue = ChapterDialogues.Exists(e => 
                e.ChapterIndex >= currentDialogueOpenerIndex && e.ChapterIndex <= currentChapterIndex);

            return hasRegularDialogue;
        }

        public bool HasPendingSRankDialogue()
        {
            if (_overrideSaveData)
                return _overriddenSRankEntry != null;

            bool hasSRankDialogue = FlagManager.Get(ProgressFlag.CurrentSRankDialogueIndex)
                                    < FlagManager.Get(ProgressFlag.SRankCount);

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
                FlagManager.Increment(ProgressFlag.SRankCount);
            }
        }

        [Button("Clear S Ranks On All Chapter")]
        public void ClearSRanksOnAllChapters()
        {
            SaveManager.GameStore.SetList(DialogueSaveKeys.SRankAchievedListKey, new List<int>());
            FlagManager.Set(ProgressFlag.SRankCount, 0);
        }
    }
}