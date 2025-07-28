using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "DialogueStarter", menuName = "CharonsCorner/Dialogue/Dialogue Starter", order = 0)]
    public class DialogueStarterSO : ScriptableObject
    {
        [field: SerializeField] public DialogueReaction Reaction { get; private set; }
        [field: SerializeField] public string SpeakerName { get; private set; } = string.Empty;
        [field: SerializeField, TextArea(5, 20)] public string Text { get; private set; } = string.Empty;

        [field: SerializeField, SerializedDictionary("Option Name", "Next Dialogue")]
        public SerializedDictionary<string, DialogueSO> Options { get; private set; } = new SerializedDictionary<string, DialogueSO>();
    }
}
