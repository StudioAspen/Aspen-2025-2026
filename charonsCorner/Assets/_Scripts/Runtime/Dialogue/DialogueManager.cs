using DG.Tweening;
using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Febucci.TextAnimatorForUnity;
using MoreMountains.Tools;

namespace CharonsCorner.Runtime
{
    public class DialogueManager : Singleton<DialogueManager>
    {
        [field: SerializeField, ReadOnly] public DialogueOpenerSO CurrentOpener { get; private set; }
        public event Action<DialogueOpenerSO> OnDialogueOpenerStarted = delegate { };

        [field: SerializeField, ReadOnly] public DialogueSequenceSO CurrentSequence { get; private set; }
        public event Action<DialogueSequenceSO> OnDialogueSequenceStarted = delegate { };
        public event Action<DialogueSequenceSO, string> OnDialogueSequenceEndReached = delegate { };

        [field: SerializeField, ReadOnly] public int CurrentDialogueIndex { get; private set; }
        [field: SerializeField, ReadOnly] public DialogueSO CurrentDialogue { get; private set; }
        [field: SerializeField, ReadOnly] public string CurrentLine { get; private set; }
        
        [Header("S-Rank Events")]
        [SerializeField] private string _sRankUIEnterEventName = "SRankUIEnter";
        [SerializeField] private string _sRankUIExitEventName = "SRankUIExit";
        
        private bool _isSRankActive;
        public bool IsSRankActive 
        { 
            get => _isSRankActive;
            set
            {
                if (_isSRankActive == value) return;
                _isSRankActive = value;
                if (_isSRankActive)
                {
                    MMGameEvent.Trigger(_sRankUIEnterEventName);
                }
            }
        }
        
        public event Action<DialogueSO> OnDialogueStarted = delegate { };
        public event Action<string> OnLineStarted = delegate { };

        public event Action OnDialogueEnded = delegate { };
        
        public MonoBehaviour Owner { get; private set; }
        public DialogueBacklog CurrentBacklog { get; private set; }
        public Action ReturnAction { get; private set; }

        public void SetOwner(MonoBehaviour owner) => Owner = owner;
        
        public void SetBacklog(DialogueBacklog backlog) => CurrentBacklog = backlog;
        
        public void SetReturnAction(Action returnAction)
        {
            ReturnAction = returnAction;
        }
        
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
            
            if (opener.SequenceOptions != null && opener.SequenceOptions.Count > 0)
            {
                StartDialogueSequence(opener.SequenceOptions[0]);
            }
            else
            {
                Debug.LogWarning($"DialogueOpenerSO {opener.name} has no sequences.");
            }
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
            if (sequence.lines != null && sequence.lines.Length > 0)
            {
                TriggerTalkEvent(sequence.lines[CurrentDialogueIndex].speaker);
                TriggerAnimationEvents(sequence.lines[CurrentDialogueIndex]);
                StartLine(GetProcessedLine(sequence.lines[CurrentDialogueIndex]));

                if (CurrentDialogueIndex >= sequence.lines.Length - 1)
                    OnDialogueSequenceEndReached.Invoke(CurrentSequence, CurrentLine);
            }
            else
            {
                Debug.LogWarning($"DialogueSequenceSO {sequence.SequenceName} has no lines.");
                OnDialogueSequenceEndReached.Invoke(CurrentSequence, string.Empty);
            }
        }

        private string GetProcessedLine(DialogueLine line)
        {
            string speakerName = line.speaker switch
            {
                Speaker.Charon => "Charon",
                Speaker.Bowley => "Bowley",
                Speaker.Unknown => "???",
                Speaker.LostMemento => "???",
                _ => "???"
            };

            return $"<?ChangeSpeakerName={speakerName}>{line.text}";
        }

        public void StartDialogue(DialogueSO dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("DialogueSO is null. Cannot start dialogue.");
                return;
            }

            if (dialogue.SpeakerName == "Charon")
                MMGameEvent.Trigger("CharonTalk");
            else if (dialogue.SpeakerName == "Bowley")
                MMGameEvent.Trigger("BowleyTalk");

            CurrentDialogue = dialogue;
            CurrentLine = dialogue.Text;
            OnDialogueStarted.Invoke(dialogue);
        }

        public void StartLine(string line)
        {
            CurrentLine = line;
            OnLineStarted.Invoke(line);
        }

        public void StartNextDialogueInSequence()
        {
            if(CurrentSequence == null)
            {
                Debug.LogWarning("CurrentSequence is null. Cannot start next dialogue in sequence.");
                return;
            }

            if (CurrentDialogueIndex + 1 < CurrentSequence.lines.Length)
            {
                CurrentDialogueIndex++;
                TriggerTalkEvent(CurrentSequence.lines[CurrentDialogueIndex].speaker);
                TriggerAnimationEvents(CurrentSequence.lines[CurrentDialogueIndex]);
                string nextLine = GetProcessedLine(CurrentSequence.lines[CurrentDialogueIndex]);
                StartLine(nextLine);

                if(CurrentDialogueIndex >= CurrentSequence.lines.Length - 1)
                    OnDialogueSequenceEndReached.Invoke(CurrentSequence, nextLine);
            }
            else
            {
                // End of current sequence. Check if there's another sequence in the opener.
                if (CurrentOpener != null)
                {
                    int nextSequenceIndex = CurrentOpener.SequenceOptions.IndexOf(CurrentSequence) + 1;
                    if (nextSequenceIndex > 0 && nextSequenceIndex < CurrentOpener.SequenceOptions.Count)
                    {
                        StartDialogueSequence(CurrentOpener.SequenceOptions[nextSequenceIndex]);
                        return;
                    }
                }
                
                Debug.LogWarning("End of all dialogue sequences reached.");
            }
        }

        private void TriggerTalkEvent(Speaker speaker)
        {
            switch (speaker)
            {
                case Speaker.Charon:
                    MMGameEvent.Trigger("CharonTalk");
                    break;
                case Speaker.Bowley:
                    MMGameEvent.Trigger("BowleyTalk");
                    break;
            }
        }

        private void TriggerAnimationEvents(DialogueLine line)
        {
            if (!string.IsNullOrEmpty(line.charonAnimation))
            {
                MMGameEvent.Trigger(line.charonAnimation);
            }

            if (!string.IsNullOrEmpty(line.charonStareAnimation))
            {
                MMGameEvent.Trigger(line.charonStareAnimation);
            }

            if (!string.IsNullOrEmpty(line.bowleyAnimation))
            {
                MMGameEvent.Trigger(line.bowleyAnimation);
            }
        }

        public void EndDialogue()
        {
            if (IsSRankActive)
            {
                MMGameEvent.Trigger(_sRankUIExitEventName);
                IsSRankActive = false;
                MMGameEvent.Trigger("SRankEnd");
            }

            OnDialogueEnded.Invoke();
            MMGameEvent.Trigger("OnDialogueEnd");
            
            CurrentOpener = null;
            CurrentSequence = null;
            CurrentDialogueIndex = 0;
            CurrentDialogue = null;
            CurrentLine = string.Empty;
            
            Owner = null;
            CurrentBacklog = null;
            ReturnAction = null;
        }

        public void ChangeSpeakerName(TMPro.TMP_Text nameText, TypewriterComponent nameTypewriter, string newName)
        {
            if (nameText == null) return;
            if (nameText.text == newName)
            {
                // Ensure name is visible if it was already set but typewriter didn't run
                if (nameTypewriter != null)
                {
                }
                return;
            }

            nameText.text = newName;
            if (nameTypewriter != null)
            {
                nameTypewriter.ShowText(newName);
            }
        }
    }
}
