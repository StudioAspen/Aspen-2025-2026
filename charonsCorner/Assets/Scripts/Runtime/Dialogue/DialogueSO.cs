using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "CharonsCorner/Dialogue/Dialogue", order = 1)]
    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string SpeakerName { get; private set; } = string.Empty;
        [field: SerializeField, TextArea(5, 20)] public string Text { get; private set; } = string.Empty;

        [field: Space(20f)]
        [field: SerializeField] public DialogueReaction Reaction { get; private set; }

        [field: Space(20f)]
        [field: SerializeField] public List<DialogueEffectConfigContainer> Effects { get; private set; } = new();
        [System.Serializable]
        public class DialogueEffectConfigContainer
        {
            [SerializeReference] public DialogueEffectConfig Config;
        }
    }
}
