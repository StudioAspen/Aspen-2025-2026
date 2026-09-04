using Animancer;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Simple utility script to play audio via the AudioManager, specifically designed for use with UnityEvents.
    /// </summary>
    public class PlayAudio : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private AudioManager.MixerTarget _mixerOverride = AudioManager.MixerTarget.Default;
        [SerializeField] [Range(0.1f, 3f)] private float _pitch = 1f;

        /// <summary>
        /// Plays the specified audio ID using the global AudioManager.
        /// Can be called from UnityEvents.
        /// </summary>
        /// <param name="audioId">The StringAsset ID of the audio clip to play.</param>
        public void Play(StringAsset audioId)
        {
            if (audioId == null)
            {
                Debug.LogWarning($"[PlayAudio] {name} tried to play a null audio ID.");
                return;
            }

            if (AudioManager.Instance == null)
            {
                Debug.LogError("[PlayAudio] AudioManager instance not found in scene!");
                return;
            }

            AudioManager.Instance.Play(audioId, _mixerOverride, pitch: _pitch);
        }

        /// <summary>
        /// Plays the specified audio ID at the current object's position.
        /// </summary>
        /// <param name="audioId">The StringAsset ID of the audio clip to play.</param>
        public void PlayAtPosition(StringAsset audioId)
        {
            if (audioId == null) return;
            
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.Play(audioId, _mixerOverride, position: transform.position, pitch: _pitch);
        }
    }
}
