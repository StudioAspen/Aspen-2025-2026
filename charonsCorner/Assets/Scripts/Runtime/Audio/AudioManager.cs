using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Audio;

namespace CharonsCorner.Runtime
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource _musicSource;

        [field: Header("Mixers")]
        [field: SerializeField] public AudioMixer MasterMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup SfxMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup UiMixer { get; private set; }
        [field: SerializeField] public AudioMixerGroup MusicMixer { get; private set; }
        [SerializeField] private DefaultMixerTarget _defaultMixer = DefaultMixerTarget.None;

        public static readonly string MasterVolumeParam = "MasterVolume";
        public static readonly string SfxVolumeParam = "SFXVolume";
        public static readonly string UIVolumeParam = "UIVolume";
        public static readonly string MusicVolumeParam = "MusicVolume";
    
        [Space]
        [SerializeField, SerializedDictionary("Audio ID", "Audio Clip")]
        private SerializedDictionary<StringAsset, AudioClip> _soundBank;
        [SerializeField, SerializedDictionary("Audio ID", "Audio Clip")]
        private SerializedDictionary<StringAsset, AudioClip> _musicBank;
    
        private readonly Dictionary<StringAsset, int> _lastPlayedFrame = new();

        public enum MixerTarget
        {
            None,
            Default,
            SFX,
            UI
        }

        public enum DefaultMixerTarget
        {
            None = MixerTarget.None,
            SFX = MixerTarget.SFX,
            UI = MixerTarget.UI
        }
        
        public void Play(StringAsset clip, MixerTarget mixerTarget, Vector3? position = null, float pitch = 1f, bool persistAcrossScenes = false)
        {
            // Prevent same sound from playing twice in the same frame
            int frame = Time.frameCount;
            if (_lastPlayedFrame.TryGetValue(clip, out int lastFrame) && lastFrame == frame)
                return;
            _lastPlayedFrame[clip] = frame;
            
            if (_soundBank.TryGetValue(clip, out AudioClip audioClip))
            {
                GameObject clipObject = new GameObject(clip, typeof(AudioDestroyer));
                if(persistAcrossScenes)
                    DontDestroyOnLoad(clipObject);
                AudioSource source = clipObject.AddComponent<AudioSource>();
                if (position.HasValue)
                {
                    clipObject.transform.position = position.Value;
                    source.spatialBlend = 1;
                    source.rolloffMode = AudioRolloffMode.Linear;
                    source.maxDistance = 20f;
                    source.dopplerLevel = 0f;
                }
                source.clip = audioClip;
                source.pitch = pitch;
                source.outputAudioMixerGroup = GetMixerGroup(mixerTarget);
                source.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip '{clip}' not found in sound bank");
            }
        }
        
        public void Play(StringAsset clip, Vector3? position = null, float pitch = 1.0f)
        {
            Play(clip, MixerTarget.Default, position, pitch);
        }

        public void PlayAndFollow(StringAsset clip, Transform target, MixerTarget mixerTarget)
        {
            if (_soundBank.TryGetValue(clip, out AudioClip audioClip))
            {
                GameObject clipObject = new GameObject(clip, typeof(AudioDestroyer));
                AudioSource source = clipObject.AddComponent<AudioSource>();
                FollowTarget followTarget = clipObject.AddComponent<FollowTarget>();
                source.spatialBlend = 1;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.maxDistance = 50f;
                source.dopplerLevel = 0f;
                source.clip = audioClip;
                source.outputAudioMixerGroup = GetMixerGroup(mixerTarget);
                followTarget.Init(target, FollowTarget.UpdateMode.Late);
                source.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip '{clip}' not found in sound bank");
            }
        }

        public void PlayMusic(StringAsset music)
        {
            if (music == null)
                return;

            if (_musicBank.TryGetValue(music, out AudioClip audioClip))
            {
                _musicSource.clip = audioClip;
                _musicSource.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip '{music}' not present in music bank");
            }
        }

        public void PauseMusic() => _musicSource.Pause();

        public void UnpauseMusic() => _musicSource.UnPause();
        
        public void StopMusic()
        {
            _musicSource.Stop();
            _musicSource.clip = null;
        }
        
        private AudioMixerGroup GetMixerGroup(MixerTarget target)
        {
            if (target == MixerTarget.None) return null;
            if (target == MixerTarget.Default) return GetMixerGroup((MixerTarget)_defaultMixer);
            if (target == MixerTarget.SFX) return SfxMixer;
            if (target == MixerTarget.UI) return UiMixer;
            throw new System.Exception("Invalid MixerTarget");
        }
        
        public static float ConvertFloatToDecibels(float value)
        {
            if (value == 0) return -80;
            return Mathf.Log10(value) * 20;
        }

        public static float ConvertDecibelsToFloat(float db)
        {
            if (db == -80) return 0;
            return Mathf.Pow(10, db / 20);
        }

        public float GetFloatNormalized(string param)
        {
            if (MasterMixer.GetFloat(param, out float v)) return ConvertDecibelsToFloat(v);
            return -1;
        }

        public static void SetMixerVolume(string mixerParamName, float volumeTarget)
        {
            Instance.MasterMixer.SetFloat(mixerParamName, ConvertFloatToDecibels(volumeTarget));
        }
    }
}