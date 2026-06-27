using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "CharonsCorner/Dialogue/Dialogue Sequence", order = 2)]
    public class DialogueSequenceSO : ScriptableObject
    {
        [field: SerializeField] public string SequenceName { get; private set; } = string.Empty;
        public string[] lines;
    }
}
