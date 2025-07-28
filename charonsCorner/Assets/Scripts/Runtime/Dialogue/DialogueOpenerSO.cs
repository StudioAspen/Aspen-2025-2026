using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "DialogueOpener", menuName = "CharonsCorner/Dialogue/Dialogue Opener", order = 1)]
    public class DialogueOpenerSO : DialogueSO
    {
        [field: SerializeField] public List<DialogueSequenceSO> SequenceOptions { get; private set; } = new();
    }
}
