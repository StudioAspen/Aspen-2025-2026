using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TestDialogueStarter : MonoBehaviour
    {
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private DialogueSO dialogue;

        [Button("Play Dialogue")]
        public void PlayDialogue()
        {
            dialogueManager.StartDialogue(dialogue);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
