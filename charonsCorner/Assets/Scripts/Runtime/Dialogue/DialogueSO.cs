using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "CharonsCorner/Dialogue/Dialogue", order = 1)]
    public class DialogueSO : ScriptableObject
    {
        [field: SerializeField] public string SpeakerName { get; private set; } = string.Empty;
        [field: SerializeField, TextArea(5, 20)] public string Text { get; private set; } = string.Empty;
    }
}
