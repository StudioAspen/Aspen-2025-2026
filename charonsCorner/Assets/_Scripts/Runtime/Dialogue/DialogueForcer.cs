using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [HideMonoScript]
    public class DialogueForcer : MonoBehaviour
    {
        [Required]
        [SerializeField] private DialogueBacklog _backlog;

        [Title("Controls")]
        [ShowIf(nameof(_backlog))]
        [Button(ButtonSizes.Large)]
        [GUIColor(1, 0.5f, 0.5f)]
        public void ClearAllOverrides()
        {
            if (_backlog != null)
            {
                // We might need a public method in DialogueBacklog to clear overrides easily
                // For now, let's assume we can just set them to null if we make a method there.
                // Actually, I'll just call a new method I'll add to DialogueBacklog.
                _backlog.ClearOverrides();
            }
        }

        [Title("Chapter Dialogues")]
        [ShowIf(nameof(_backlog))]
        [ListDrawerSettings(IsReadOnly = true, Expanded = true)]
        public List<ChapterDialogueEntry> ChapterDialogues => _backlog != null ? _backlog.ChapterDialogues : null;

        [Title("S-Rank Dialogues")]
        [ShowIf(nameof(_backlog))]
        [ListDrawerSettings(IsReadOnly = true, Expanded = true)]
        public List<ChapterSRankDialogueEntry> SRankDialogues => _backlog != null ? _backlog.SRankDialogues : null;

        private void Reset()
        {
            if (_backlog == null)
            {
                _backlog = Object.FindAnyObjectByType<DialogueBacklog>();
            }
        }
    }
}
