using Animancer;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager dialogueManager;

        [Header("References")]
        [SerializeField] private AnimancerComponent animator;

        [Header("Config")]
        [SerializeField] private float animatorFadeDuration = 0.2f;

        [SerializeField, SerializedDictionary("Reaction", "Animation Clip")]
        private SerializedDictionary<DialogueReaction, ClipTransition> reactionAnimations;

        private void Awake()
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);

            dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;
        }

        private void Start()
        {
            animator.Play(reactionAnimations[DialogueReaction.Idle], animatorFadeDuration);
        }

        private void OnDestroy()
        {
            if(dialogueManager != null)
            {
                dialogueManager.OnDialogueOpenerStarted -= DialogueManager_OnDialogueOpenerStarted;
                dialogueManager.OnDialogueStarted -= DialogueManager_OnDialogueStarted;
                dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;
            }
        }

        private void DialogueManager_OnDialogueOpenerStarted(DialogueOpenerSO opener)
        {
            animator.Play(reactionAnimations[opener.Reaction], animatorFadeDuration);
            opener.ApplyEffects();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            animator.Play(reactionAnimations[dialogue.Reaction], animatorFadeDuration);
            dialogue.ApplyEffects();
        }

        private void DialogueManager_OnDialogueEnded()
        {
            animator.Play(reactionAnimations[DialogueReaction.Idle], animatorFadeDuration);
        }
    }
}
