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
        public AudioSource MusicSource => _musicSource;

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
        [SerializeField]
        private AudioBankSO _soundBank;
        [SerializeField]
        private AudioBankSO _musicBank;
    
        private readonly Dictionary<StringAsset, int> _lastPlayedFrame = new();

        public enum MixerTarget
        {
            None,
            Default,
            SFX,
            UI,
            Music
        }

        public enum DefaultMixerTarget
        {
            None = MixerTarget.None,
            SFX = MixerTarget.SFX,
            UI = MixerTarget.UI
        }
        
        public void Play(StringAsset clip, MixerTarget mixerTarget = MixerTarget.Default, Vector3? position = null, float pitch = 1f, bool persistAcrossScenes = false)
        {
            // Prevent same sound from playing twice in the same frame
            int frame = Time.frameCount;
            if (_lastPlayedFrame.TryGetValue(clip, out int lastFrame) && lastFrame == frame)
                return;
            _lastPlayedFrame[clip] = frame;
            
            if (_soundBank.Bank.TryGetValue(clip, out AudioEntry entry))
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
                source.clip = entry.Clip;
                source.pitch = pitch;
                source.volume = entry.Volume;

                // Resolve mixer: Use provided override, otherwise use bank setting
                MixerTarget finalTarget = mixerTarget == MixerTarget.Default ? entry.Mixer : mixerTarget;
                source.outputAudioMixerGroup = GetMixerGroup(finalTarget);
                
                source.Play();
            }
            else
            {
                Debug.LogWarning($"AudioClip '{clip}' not found in sound bank");
            }
        }
        
        // Removed old Play overload to avoid ambiguity with default parameters

        public void PlayAndFollow(StringAsset clip, Transform target, MixerTarget mixerTarget = MixerTarget.Default)
        {
            if (_soundBank.Bank.TryGetValue(clip, out AudioEntry entry))
            {
                GameObject clipObject = new GameObject(clip, typeof(AudioDestroyer));
                AudioSource source = clipObject.AddComponent<AudioSource>();
                FollowTarget followTarget = clipObject.AddComponent<FollowTarget>();
                source.spatialBlend = 1;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.maxDistance = 50f;
                source.dopplerLevel = 0f;
                source.clip = entry.Clip;
                source.volume = entry.Volume;

                // Resolve mixer: Use provided override, otherwise use bank setting
                MixerTarget finalTarget = mixerTarget == MixerTarget.Default ? entry.Mixer : mixerTarget;
                source.outputAudioMixerGroup = GetMixerGroup(finalTarget);

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

            if (_musicBank.Bank.TryGetValue(music, out AudioEntry entry))
            {
                _musicSource.clip = entry.Clip;
                _musicSource.volume = entry.Volume;
                
                // Resolve mixer: Use bank setting if it's not Default, otherwise keep current MusicSource setting
                if (entry.Mixer != MixerTarget.Default && entry.Mixer != MixerTarget.None)
                {
                    _musicSource.outputAudioMixerGroup = GetMixerGroup(entry.Mixer);
                }
                else if (entry.Mixer == MixerTarget.Default && _musicSource.outputAudioMixerGroup == null)
                {
                    // Fallback to MusicMixer if source has no mixer and entry is Default
                    _musicSource.outputAudioMixerGroup = MusicMixer;
                }
                
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
        
        public AudioMixerGroup GetMixerGroup(MixerTarget target)
        {
            if (target == MixerTarget.None) return null;
            if (target == MixerTarget.Default) return GetMixerGroup((MixerTarget)_defaultMixer);
            if (target == MixerTarget.SFX) return SfxMixer;
            if (target == MixerTarget.UI) return UiMixer;
            if (target == MixerTarget.Music) return MusicMixer;
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
        
        public bool TryGetClip(StringAsset id, out AudioClip clip)
        {
            if (_soundBank.Bank.TryGetValue(id, out AudioEntry entry))
            {
                clip = entry.Clip;
                return true;
            }

            clip = null;
            return false;
        }
    }
}