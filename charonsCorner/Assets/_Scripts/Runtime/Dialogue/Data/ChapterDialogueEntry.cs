using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class ChapterDialogueEntry
    {
        public int ChapterIndex;
        public DialogueOpenerSO DialogueOpener;

        [Button("Force This Dialogue")]
        [GUIColor(0, 1, 0)]
        private void ForceThisDialogue()
        {
            DialogueBacklog backlog = Object.FindAnyObjectByType<DialogueBacklog>();
            if (backlog != null)
            {
                backlog.SetOverriddenDialogue(this);
            }
        }
    }
}