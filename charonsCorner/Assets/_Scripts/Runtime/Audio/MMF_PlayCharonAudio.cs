using System;
using UnityEngine;
using MoreMountains.Feedbacks;
using Animancer;
using CharonsCorner.Runtime;

namespace MoreMountains.Feedbacks
{
    [System.Serializable]
    [FeedbackPath("Audio/Play Charon Audio")]
    [FeedbackHelp("This feedback lets you play audio using the Charon AudioManager and StringAssets.")]
    public class MMF_PlayCharonAudio : MMF_Feedback
    {
        /// a static bool used to disable all feedbacks of this type at once
        public static bool FeedbackTypeAuthorized = true;
        
        #if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
        public override bool EvaluateRequiresSetup() { return (AudioId == null && (RandomAudioIds == null || RandomAudioIds.Length == 0)); }
        public override string RequiredTargetText { get { return AudioId != null ? AudioId.name : (RandomAudioIds != null && RandomAudioIds.Length > 0 ? "Random" : ""); } }
        public override string RequiresSetupText { get { return "This feedback requires that an AudioId or RandomAudioIds be set to be able to work properly."; } }
        #endif

        [MMFInspectorGroup("Charon Audio", true, 5)]
        /// the audio ID to play (as a StringAsset)
        [Tooltip("the audio ID to play (as a StringAsset)")]
        public StringAsset AudioId;
        
        /// an array to pick a random audio ID from
        [Tooltip("an array to pick a random audio ID from")]
        public StringAsset[] RandomAudioIds;

        [MMFInspectorGroup("Audio Settings", true, 6)]
        /// the mixer to play the sound with (optional override)
        [Tooltip("the mixer to play the sound with (optional override)")]
        public AudioManager.MixerTarget MixerOverride = AudioManager.MixerTarget.Default;

        /// the pitch to play the sound at
        [Tooltip("the pitch to play the sound at")]
        [Range(0.1f, 3f)]
        public float Pitch = 1f;

        /// whether or not to play the sound at the feedback's position
        [Tooltip("whether or not to play the sound at the feedback's position")]
        public bool PlayAtPosition = false;

        /// the range of the sound if played at position
        [Tooltip("the range of the sound if played at position")]
        [MMFCondition("PlayAtPosition", true)]
        public float Range = 20f;

        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
        {
            if (!Active || !FeedbackTypeAuthorized)
            {
                return;
            }

            StringAsset idToPlay = AudioId;

            if ((RandomAudioIds != null) && (RandomAudioIds.Length > 0))
            {
                idToPlay = RandomAudioIds[UnityEngine.Random.Range(0, RandomAudioIds.Length)];
            }

            if (idToPlay == null)
            {
                return;
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[MMF_PlayCharonAudio] AudioManager instance not found!");
                return;
            }

            if (PlayAtPosition)
            {
                AudioManager.Instance.Play(idToPlay, MixerOverride, position: position, pitch: Pitch, maxDistance: Range);
            }
            else
            {
                AudioManager.Instance.Play(idToPlay, MixerOverride, pitch: Pitch);
            }
        }
    }
}
