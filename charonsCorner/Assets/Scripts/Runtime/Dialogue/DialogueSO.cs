using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "CharonsCorner/Dialogue/Dialogue", order = 1)]
    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public DialogueReaction Reaction { get; private set; }
        [field: SerializeField] public string SpeakerName { get; private set; } = string.Empty;
        [field: SerializeField] public List<DialogueTextContainer> Texts { get; private set; } = new();

        /// <summary>
        /// We need to containerize the text to allow for serialization of list of multiline text fields in the inspector.
        /// </summary>
        [System.Serializable]
        public class DialogueTextContainer
        {
            [field: SerializeField, TextArea(5, 20)] public string Text { get; private set; } = string.Empty;
        }
    }
}
