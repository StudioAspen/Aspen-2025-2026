using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "CharonsCorner/Dialogue/Dialogue", order = 0)]
    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; } = string.Empty;
        [field: SerializeField, TextArea(5, 20)] public string Text { get; private set; } = string.Empty;
        [field: SerializeField] public DialogueReaction Reaction { get; private set; }
        [field: SerializeField, SerializedDictionary("Option Name", "Next Dialogue")] 
        public SerializedDictionary<string, DialogueSO> Options { get; private set; } = new SerializedDictionary<string, DialogueSO>();
    }
}
