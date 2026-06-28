using UnityEngine;
using Febucci.TextAnimatorForUnity;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Holds references to components on the Dialogue Canvas to be accessed by DialogueSequenceEventSubscriber.
    /// This script should be placed on the DialogueCanvas prefab.
    /// </summary>
    public class DialogueCanvasReferencePasserForEventSubscriber : Singleton<DialogueCanvasReferencePasserForEventSubscriber>
    {
        [Header("References")]
        [SerializeField] private TypewriterComponent typewriter;
        [SerializeField] private TMPro.TMP_Text speakerNameText;
        [SerializeField] private TypewriterComponent speakerNameTypewriter;

        public TypewriterComponent Typewriter => typewriter;
        public TMPro.TMP_Text SpeakerNameText => speakerNameText;
        public TypewriterComponent SpeakerNameTypewriter => speakerNameTypewriter;
    }
}
