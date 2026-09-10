using MoreMountains.Tools;
using UnityEngine;
using Animancer;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Plays a specified audio clip from the AudioManager when a specific MMGameEvent is triggered.
    /// </summary>
    public class PlayAudioOnMMGameEvent : MonoBehaviour, MMEventListener<MMGameEvent>
    {
        [Tooltip("The name of the MMGameEvent that will trigger the audio.")]
        [SerializeField] private string _eventName;

        [Tooltip("The audio clip to play (from the Sound Bank).")]
        [SerializeField] private StringAsset _audioClip;

        [Tooltip("The mixer channel to play the audio in. Default will use the channel specified in the Sound Bank.")]
        [SerializeField] private AudioManager.MixerTarget _mixerTarget = AudioManager.MixerTarget.Default;

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
            if (gameEvent.EventName == _eventName && _audioClip != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play(_audioClip, _mixerTarget);
                }
            }
        }
    }
}
