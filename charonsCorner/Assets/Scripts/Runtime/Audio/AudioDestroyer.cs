using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class AudioDestroyer : MonoBehaviour
    {
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_source == null)
            {
                _source = GetComponent<AudioSource>();
                return;
            }

            if (_source.timeSamples == _source.clip.samples || !_source.isPlaying)
                Destroy(gameObject);
        }
    }
}