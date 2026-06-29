using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CharonsCorner.Runtime
{
    public enum Speaker
    {
        Charon,
        Bowley,
        Unknown // Mapping ??? to Unknown in code for better naming, but will display as ??? in inspector if possible or just use string mapping
    }

    [System.Serializable]
    public struct DialogueLine
    {
        [HideLabel]
        public Speaker speaker;
        [HideLabel]
        [TextArea(3, 10)]
        public string text;
    }

    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "CharonsCorner/Dialogue/Dialogue Sequence", order = 2)]
    public class DialogueSequenceSO : ScriptableObject
    {
        [field: SerializeField] public string SequenceName { get; private set; } = string.Empty;
        public DialogueLine[] lines;
    }
}
