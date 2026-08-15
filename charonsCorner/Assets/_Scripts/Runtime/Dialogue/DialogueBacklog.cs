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
            if (_overrideSaveData)
            {
                if (_overriddenDialogueEntry != null)
                {
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
                        overriddenSequences.Add(_overriddenDialogueEntry.DialogueOpener.SequenceOptions[sequenceIndex]);
                    }
                    else if (_overriddenDialogueEntry.DialogueOpener.SequenceOptions.Count > 0)
                    {
                        // Play exhausted sequence (last one) if all sequences are done
                        overriddenSequences.Add(_overriddenDialogueEntry.DialogueOpener.SequenceOptions[_overriddenDialogueEntry.DialogueOpener.SequenceOptions.Count - 1]);
                    }

                    runtimeOpener.SetSequenceOptions(overriddenSequences);
                    return runtimeOpener;
                }
                
                if (_overriddenSRankEntry != null)
                {
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
            int currDialogueOpenerIndex = FlagManager.Get(CurrentDialogueOpenerIndexFlag);
            
            if (HasPendingRegularDialogue())
            {
                entry = ChapterDialogues.Find(e => e.ChapterIndex == currDialogueOpenerIndex);
                if (entry == null)
                    Debug.LogWarning($"No dialogue entry found for ChapterIndex {currDialogueOpenerIndex}");
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
                int lastIndex = Mathf.Clamp(currDialogueOpenerIndex - 1, 0, ChapterDialogues.Count - 1);
                if (ChapterDialogues[lastIndex].DialogueOpener == null)
                {
                    Debug.LogError($"[DialogueBacklog] DialogueOpener is null for chapter {ChapterDialogues[lastIndex].ChapterIndex} (fallback)!");
                    return null;
                }
                runtimeOpener = Instantiate(ChapterDialogues[lastIndex].DialogueOpener);
            }
            else
            {
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
                    sequencesToPlay.Add(entry.DialogueOpener.SequenceOptions[sequenceIndex]);
                }
            }

            // 2. S-Rank Sequences (only if regular sequences for the current chapter are done)
            if (sRankEntry != null && sequencesToPlay.Count == 0)
            {
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
                    sequencesToPlay.Add(runtimeOpener.SequenceOptions[runtimeOpener.SequenceOptions.Count - 1]);
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
            FlagManager.Increment(CurrentDialogueOpenerIndexFlag);
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

            int currentChapterIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            bool hasRegularDialogue = FlagManager.Get(CurrentDialogueOpenerIndexFlag)
                                      <= currentChapterIndex;

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