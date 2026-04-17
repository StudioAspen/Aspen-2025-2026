using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class AudioTrig : MonoBehaviour
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

