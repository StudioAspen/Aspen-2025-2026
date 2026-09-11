using System.Collections.Generic;
using Animancer;
using MoreMountains.Tools;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Plays a random audio clip from a list of audio IDs when PlayBlip is called.
    /// Can intake multiple lists triggered by MMGameEvents.
    /// Used for dialogue blips or similar text-related sound effects.
    /// </summary>
    public class PlayTextBlipAudio : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [System.Serializable]
        public class BlipAudioList
        {
            [Tooltip("The name of the MMGameEvent that will switch to this audio list.")]
            public string EventName;
            [Tooltip("List of audio IDs to play when this event is active.")]
            public List<StringAsset> AudioIds = new List<StringAsset>();
        }

        [Header("Audio Lists")]
        [SerializeField] private List<BlipAudioList> _blipAudioLists = new List<BlipAudioList>();
        
        [Header("Settings")]
        [SerializeField] private AudioManager.MixerTarget _mixerTarget = AudioManager.MixerTarget.UI;
        
        [Header("Pitch Randomization")]
        [SerializeField] private bool _useRandomPitch = true;
        [SerializeField] [Range(0.1f, 2f)] private float _minPitch = 0.9f;
        [SerializeField] [Range(0.1f, 2f)] private float _maxPitch = 1.1f;

        private BlipAudioList _currentList;

        private void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
        }

        public void OnMMEvent(MMGameEvent gameEvent)
        {
            foreach (var audioList in _blipAudioLists)
            {
                if (audioList.EventName == gameEvent.EventName)
                {
                    _currentList = audioList;
                    break;
                }
            }
        }

        /// <summary>
        /// Plays a random blip sound from the currently active list.
        /// Defaults to the first list if no event has been received.
        /// </summary>
        public void PlayBlip()
        {
            if (_currentList == null && _blipAudioLists.Count > 0)
            {
                _currentList = _blipAudioLists[0];
            }

            if (_currentList == null || _currentList.AudioIds == null || _currentList.AudioIds.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, _currentList.AudioIds.Count);
            StringAsset selectedBlip = _currentList.AudioIds[randomIndex];

            if (selectedBlip == null)
            {
                return;
            }

            float pitch = 1f;
            if (_useRandomPitch)
            {
                pitch = Random.Range(_minPitch, _maxPitch);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(selectedBlip, _mixerTarget, pitch: pitch);
            }
            else
            {
                Debug.LogWarning("AudioManager instance not found. Cannot play blip.");
            }
        }
    }
}
