using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "NewFlashbackDialogue", menuName = "Dialogue/FlashbackDialogueData")]
    public class FlashbackDialogueData : ScriptableObject
    {
        public string[] lines;
    }
}
