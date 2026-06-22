using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;

using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayNewAudio : MonoBehaviour
    {
        [SerializeField] private StringAsset _targetMusicId;
        [SerializeField] private bool _stopCurrentMusic = true;
        [SerializeField] private bool _playOnce = true;

        private AudioSource _musicSource;
        private bool _hasPlayed = false;


        private void Awake()
        {
            _musicSource = AudioManager.Instance.MusicSource;
        }

        private void OnTriggerEnter(UnityEngine.Collider other)
        {
            // Make sure it's the player
            if (!other.CompareTag("Player")) return;

            // Prevent replaying if needed
            if (_playOnce && _hasPlayed) return;

            // Stop current music
            if (_stopCurrentMusic && _musicSource.isPlaying)
            {
                _musicSource.Stop();
            }

            // Play new music (your AudioManager method)
            AudioManager.Instance.PlayMusic(_targetMusicId);

            _hasPlayed = true;
            if (other.CompareTag("Player"))
            {
                _musicSource.Stop();
                _musicSource.volume = 1f;
                _musicSource.Play();
            }
        }
    }
}