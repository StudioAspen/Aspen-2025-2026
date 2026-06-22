using System;
using Animancer;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class MusicController : MonoBehaviour
    {
        [SerializeField, Required] private StringAsset _targetMusicId;
        [SerializeField] private bool _willPlayTargetMusicOnStart;
        [SerializeField, ReadOnly] private bool _isMusicPlaying;

        private AudioSource _musicSource;

        private void Awake()
        {
            _musicSource = AudioManager.Instance.MusicSource;
        }

        private void Start()
        {
            if(_willPlayTargetMusicOnStart)
                Switch();
            
            _isMusicPlaying = _musicSource.isPlaying;
        }

        [Button("Switch", ButtonSizes.Large)]
        public void Switch()
        {
            AudioManager.Instance.PlayMusic(_targetMusicId);
            
            _isMusicPlaying = _musicSource.isPlaying;
        }
        
        [Button("Pause/Unpause", ButtonSizes.Large)]
        public void TogglePause()
        {
            if(_musicSource.isPlaying) 
                AudioManager.Instance.PauseMusic();
            else
                AudioManager.Instance.UnpauseMusic();
            
            _isMusicPlaying = _musicSource.isPlaying;
        }
        
        [Button("Stop", ButtonSizes.Large)]
        public void Stop()
        {
            AudioManager.Instance.StopMusic();
            
            _isMusicPlaying = _musicSource.isPlaying;
        }
    }
}