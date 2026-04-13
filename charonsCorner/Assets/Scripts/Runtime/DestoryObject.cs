using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DestoryObject : MonoBehaviour
    {
        public GameObject targetObject;
        //public float hideTime = 2f; // seconds

        //void OnTriggerEnter(Collider other)
        //{
          //  if (other.CompareTag("Player"))
            //{
              //  StartCoroutine(HideAndShow());
            //}
        //}

        //IEnumerator HideAndShow()
        //{
          //  targetObject.SetActive(false);
            //yield return new WaitForSeconds(hideTime);
            //targetObject.SetActive(true);
        //}
        void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("StairCollider"))
        {
            Destroy(gameObject);
        }
    }
  }
}
