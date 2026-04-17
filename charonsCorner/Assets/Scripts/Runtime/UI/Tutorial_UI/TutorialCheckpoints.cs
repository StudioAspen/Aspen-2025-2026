using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A tutorial checkpoint placed in the world. When the player enters its trigger,
    /// it fires a static event that the TutorialUI manager listens to.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TutorialCheckpoint : MonoBehaviour
    {
        [field: SerializeField] public int TutorialIndex { get; private set; }
        [field: SerializeField] public string TutorialTitle { get; private set; }
        [field: SerializeField, TextArea(2, 5)] public string TutorialText { get; private set; }

        [Header("Alternative Dialogue")]
        [SerializeField] private string xboxTitle;
        [SerializeField, TextArea(2, 5)] private string xboxText;
        [SerializeField] private string psTitle;
        [SerializeField, TextArea(2, 5)] private string psText;

        public string GetTitle(InputManager.ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                case InputManager.ControlScheme.Xbox:
                    return !string.IsNullOrEmpty(xboxTitle) ? xboxTitle : TutorialTitle;
                case InputManager.ControlScheme.PS:
                    return !string.IsNullOrEmpty(psTitle) ? psTitle : TutorialTitle;
                default:
                    return TutorialTitle;
            }
        }

        public string GetText(InputManager.ControlScheme controlScheme)
        {
            switch (controlScheme)
            {
                case InputManager.ControlScheme.Xbox:
                    return !string.IsNullOrEmpty(xboxText) ? xboxText : TutorialText;
                case InputManager.ControlScheme.PS:
                    return !string.IsNullOrEmpty(psText) ? psText : TutorialText;
                default:
                    return TutorialText;
            }
        }

        public static event Action<TutorialCheckpoint> OnTutorialCheckpointHit;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Activate();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Deactivate();
        }

        [Button("Activate", ButtonSizes.Large)]
        public void Activate()
        {
            OnTutorialCheckpointHit?.Invoke(this);
        }

        public void Deactivate()
        {
            OnTutorialCheckpointHit?.Invoke(null);
        }
    }
}
