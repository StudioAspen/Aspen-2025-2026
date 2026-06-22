using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "CharonsCorner/Dialogue/Dialogue Sequence", order = 2)]
    public class DialogueSequenceSO : ScriptableObject
    {
        [field: SerializeField] public string SequenceName { get; private set; } = string.Empty;

        [System.Serializable]
        public class DialogueContainer
        {
            [field: SerializeField] public DialogueSO Dialogue { get; private set; }
        }

        [field: SerializeField] public List<DialogueContainer> DialogueContainers { get; private set; } = new();
        public List<DialogueSO> DialoguesList => DialogueContainers.ConvertAll(container => container.Dialogue);
    }
}
