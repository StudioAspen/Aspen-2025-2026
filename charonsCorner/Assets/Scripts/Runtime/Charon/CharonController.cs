using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CharonController : MonoBehaviour
    {
        private DialogueManager dialogueManager;

        [SerializeField] private Animator animator;

        [System.Serializable]
        public class AnimationParameterContainer
        {
            private Animator animator;
            [field: SerializeField, AnimatorParam("animator")] public int ParameterHash { get; private set; }
            public void SetAnimator(Animator animator) => this.animator = animator;
        }

        [SerializeField, SerializedDictionary("Reaction", "Animation Parameter")]
        private SerializedDictionary<DialogueReaction, AnimationParameterContainer> reactionAnimations;

        private void OnValidate()
        {
            foreach(AnimationParameterContainer container in reactionAnimations.Values)
                container.SetAnimator(animator);
        }

        private void Awake()
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);

            dialogueManager.OnDialogueOpenerStarted += DialogueManager_OnDialogueOpenerStarted;
            dialogueManager.OnDialogueStarted += DialogueManager_OnDialogueStarted;
            dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;
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
            animator.SetTrigger(reactionAnimations[opener.Reaction].ParameterHash);
            opener.ApplyEffects();
        }

        private void DialogueManager_OnDialogueStarted(DialogueSO dialogue)
        {
            animator.SetTrigger(reactionAnimations[dialogue.Reaction].ParameterHash);
            dialogue.ApplyEffects();
        }

        private void DialogueManager_OnDialogueEnded()
        {
            animator.SetTrigger(reactionAnimations[DialogueReaction.Idle].ParameterHash);
        }
    }
}
