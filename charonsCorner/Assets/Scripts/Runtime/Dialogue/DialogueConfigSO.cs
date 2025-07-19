using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "DialogueConfig", menuName = "CharonsCorner/Dialogue/Dialogue Config", order = 0)]
    public class DialogueConfigSO : ScriptableObject
    {
        [field: SerializeField, SerializedDictionary("Reaction", "Sprite")]
        public SerializedDictionary<DialogueReaction, Sprite> ReactionSprites { get; private set; } = new SerializedDictionary<DialogueReaction, Sprite>();
    }
}
