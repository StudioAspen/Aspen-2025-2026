using System.Collections.Generic;
using Animancer;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "AudioBank", menuName = "CharonsCorner/AudioBank")]
    public class AudioBankSO : ScriptableObject
    {
        [SerializeField, SerializedDictionary("Audio ID", "Audio Clip")]
        private SerializedDictionary<StringAsset, AudioClip> _bank;

        public Dictionary<StringAsset, AudioClip> Bank => _bank;
    }
}