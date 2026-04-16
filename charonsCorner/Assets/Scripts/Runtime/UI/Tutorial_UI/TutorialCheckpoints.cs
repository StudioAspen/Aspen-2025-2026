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
        [field: SerializeField, TextArea(2, 5)] public string TutorialText { get; private set; }

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
