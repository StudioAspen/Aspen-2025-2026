using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helper component to play one shot audios through the audio manager.
    /// </summary>
    public class OneShotAudioPlayer : MonoBehaviour
    {
        [SerializeField, Required] private StringAsset _audioId;
        [SerializeField] private AudioManager.MixerTarget _mixerTarget;
        [SerializeField] private FloatRange _pitchRange =  new FloatRange(1f, 1f);
        
        [InfoBox("Whether to use object's position as 3D audio position")]
        [SerializeField] private bool _isSpacial = false;
        
        [InfoBox("Whether the audio lives through scene changes")]
        [SerializeField] private bool _isPersistent = false;

        /// <summary>
        /// Plays one shot audio using the parameters from the component.
        /// </summary>
        [Button("Play", ButtonSizes.Large)]
        public void Play()
        {
            AudioManager.Instance.Play(
                _audioId, 
                _mixerTarget,
                _isSpacial ? transform.position : null,
                _pitchRange.RandomValue(), 
                _isPersistent
                );
        }
    }
}