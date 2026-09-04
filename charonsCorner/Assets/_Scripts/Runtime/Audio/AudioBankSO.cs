using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AudioEntry
    {
        public AudioManager.MixerTarget Mixer = AudioManager.MixerTarget.Default;
        public AudioClip Clip;
        [Range(0f, 2f)]
        public float Volume = 1f;
    }

    [CreateAssetMenu(fileName = "AudioBank", menuName = "CharonsCorner/AudioBank")]
    public class AudioBankSO : ScriptableObject
    {
        [SerializeField, SerializedDictionary("Audio ID", "Audio Entry"), DrawWithUnity]
        private SerializedDictionary<StringAsset, AudioEntry> _bank;

        public Dictionary<StringAsset, AudioEntry> Bank => _bank;

        public void AddEntry(StringAsset id, AudioClip clip)
        {
            if (_bank == null)
            {
                _bank = new SerializedDictionary<StringAsset, AudioEntry>();
            }

            if (!_bank.ContainsKey(id))
            {
                _bank.Add(id, new AudioEntry { Clip = clip, Volume = 1f, Mixer = AudioManager.MixerTarget.Default });
            }
        }

        public void RemoveNullKeys()
        {
            if (_bank == null) return;
            
            List<StringAsset> keysToRemove = new List<StringAsset>();
            foreach (var key in _bank.Keys)
            {
                if (key == null)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _bank.Remove(key);
            }
        }
    }
}