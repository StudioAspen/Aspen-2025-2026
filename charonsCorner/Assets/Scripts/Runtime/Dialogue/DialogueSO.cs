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
        [field: SerializeField] public List<DialogueEffectContainer> Effects { get; private set; } = new();
        [System.Serializable]
        public class DialogueEffectContainer
        {
            [field: SerializeReference] public DialogueEffect Effect { get; private set; }
        }

        public void ApplyEffects()
        {
            foreach(DialogueEffectContainer container in Effects)
            {
                if(container.Effect == null)
                    continue;
                container.Effect.ApplyEffect();
            }
        }
        
        public void SetSpeakerName(string speakerName) => SpeakerName = speakerName;
        public void SetText(string text) => Text = text;
    }
}
