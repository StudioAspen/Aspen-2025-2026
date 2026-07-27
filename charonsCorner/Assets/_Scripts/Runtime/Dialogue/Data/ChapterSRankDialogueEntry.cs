using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class ChapterSRankDialogueEntry
    {
        public int ChapterIndex;
        public DialogueSequenceSO DialogueSequence;

        [Button("Force This S-Rank Dialogue")]
        [GUIColor(0.5f, 1, 0.5f)]
        private void ForceThisSRankDialogue()
        {
            DialogueBacklog backlog = Object.FindAnyObjectByType<DialogueBacklog>();
            if (backlog != null)
            {
                backlog.SetOverriddenSRankDialogue(this);
            }
        }
    }
}