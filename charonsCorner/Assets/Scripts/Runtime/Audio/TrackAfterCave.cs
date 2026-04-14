using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TrackAfterCave : MonoBehaviour
    {
      
        public AudioSource audioSource;

        private void OnTriggerEnter(Collider other)
        {
            {
                
                audioSource.Play();
                
            }
        }

     
    }
}
