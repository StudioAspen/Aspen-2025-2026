using UnityEngine;
using CharonsCorner.Runtime;

namespace CharonsCorner.Runtime
{
    public class FlashbackTrigger : MonoBehaviour
    {
        private static FlashbackDialogueData _pendingDialogueData;

        public static void SetPendingDialogue(FlashbackDialogueData data)
        {
            _pendingDialogueData = data;
        }

        [SerializeField] private FlashbackDialogueData dialogueData;
        [SerializeField] private bool _triggerOnStart = true;

        private void Start()
        {
            if (_triggerOnStart)
            {
                TriggerDialogue();
            }
        }

        public void TriggerDialogue()
        {
            var dataToUse = _pendingDialogueData != null ? _pendingDialogueData : dialogueData;

            if (dataToUse != null)
            {
                FlashbackText.RequestDialogue(dataToUse);
            }
            else
            {
                Debug.LogWarning("[FlashbackTrigger] No DialogueData assigned or pending.");
            }

            // Clear the pending data after it's used
            _pendingDialogueData = null;
        }
    }
}
