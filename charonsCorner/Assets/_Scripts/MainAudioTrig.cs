using UnityEngine;
namespace CharonsCorner.Runtime
{
    public class MainAudioTrig : MonoBehaviour
    {
        public AudioSource audioSource;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
}

