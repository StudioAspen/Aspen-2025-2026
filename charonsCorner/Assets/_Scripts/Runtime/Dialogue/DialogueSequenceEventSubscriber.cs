using UnityEngine;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorCore.Typing;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using CharonsCorner.Runtime;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using System.Globalization;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Boilerplate script similar to FlashbackEventSubscriber for handling events triggered by dialogue sequences.
    /// It subscribes to a TypewriterComponent's onMessage event to execute actions based on text tags.
    /// </summary>
    public class DialogueSequenceEventSubscriber : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TypewriterComponent typewriter;
        [SerializeField] private TMPro.TMP_Text speakerNameText;
        [SerializeField] private TypewriterComponent speakerNameTypewriter;
        [SerializeField] private CameraSwitcher cameraSwitcher;

        [Header("Feedbacks")]
        [SerializeField] private MMF_Player genericFeedback;
        [SerializeField] private MMF_Player alternativeFeedback;

        [Header("Screen Shake")]
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeAmplitude = 1f;
        [SerializeField] private float shakeFrequency = 1f;
        [SerializeField] private MMChannelModes shakeChannelMode = MMChannelModes.Int;
        [SerializeField] private int shakeChannelInt = 0;
        [SerializeField] private MMChannel shakeChannelDefinition = null;

        private void OnEnable()
        {
            // Seek references from DialogueCanvasReferencePasserForEventSubscriber if not assigned
            if (DialogueCanvasReferencePasserForEventSubscriber.Instance != null)
            {
                if (typewriter == null) typewriter = DialogueCanvasReferencePasserForEventSubscriber.Instance.Typewriter;
                if (speakerNameText == null) speakerNameText = DialogueCanvasReferencePasserForEventSubscriber.Instance.SpeakerNameText;
                if (speakerNameTypewriter == null) speakerNameTypewriter = DialogueCanvasReferencePasserForEventSubscriber.Instance.SpeakerNameTypewriter;
            }

            if (typewriter != null)
            {
                typewriter.onMessage.AddListener(OnMessageReceived);
            }

            // Subscribing to main DialogueManager events if needed
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnded;
            }
        }

        private void OnDisable()
        {
            if (typewriter != null)
            {
                typewriter.onMessage.RemoveListener(OnMessageReceived);
            }

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnded;
            }
        }

        /// <summary>
        /// Handles markers sent by TextAnimator (e.g., <?tagname=param1,param2>)
        /// </summary>
        private void OnMessageReceived(EventMarker marker)
        {
            switch (marker.name)
            {
                case "PlayFeedback":
                    HandleFeedback(marker);
                    break;

                case "Shake":
                    MMCameraShakeEvent.Trigger(shakeDuration, shakeAmplitude, shakeFrequency, 0f, 0f, 0f, false, new MMChannelData(shakeChannelMode, shakeChannelInt, shakeChannelDefinition));
                    break;

                case "GameEvent":
                    if (marker.parameters.Length > 0)
                    {
                        MMGameEvent.Trigger(marker.parameters[0]);
                    }
                    break;
                
                case "GE":
                    if (marker.parameters.Length > 0)
                    {
                        MMGameEvent.Trigger(marker.parameters[0]);
                    }
                    break;

                case "ChangeSpeakerName":
                    if (marker.parameters.Length > 0)
                    {
                        DialogueManager.Instance.ChangeSpeakerName(speakerNameText, speakerNameTypewriter, marker.parameters[0]);
                    }
                    break;

                // Add more custom tags here
                
                default:
                    Debug.Log($"[DialogueSequenceEventSubscriber] Received unknown marker: {marker.name}");
                    break;
            }
        }
        

        private void HandleFeedback(EventMarker marker)
        {
            if (marker.parameters.Length > 0)
            {
                string feedbackName = marker.parameters[0].ToLower();
                switch (feedbackName)
                {
                    case "generic":
                        if (genericFeedback != null) genericFeedback.PlayFeedbacks();
                        break;
                    case "alt":
                        if (alternativeFeedback != null) alternativeFeedback.PlayFeedbacks();
                        break;
                }
            }
            else
            {
                if (genericFeedback != null) genericFeedback.PlayFeedbacks();
            }
        }

        private void HandleDialogueEnded()
        {
            // Reset state if necessary when dialogue finishes
        }
    }
}
