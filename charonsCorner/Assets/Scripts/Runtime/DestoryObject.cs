using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DestoryObject : MonoBehaviour
    {
        
        void OnCollisionEnter(Collision collision)
     {
        if (collision.gameObject.CompareTag("PlayerDeathCollider")) 
        {
            Destroy(gameObject);
        }
      }
    }
}