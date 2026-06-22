using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Trackerplayer : MonoBehaviour
    {
        public Transform targetObj;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
         transform.position = Vector3.MoveTowards(transform.position, targetObj.position, 10* Time.deltaTime);
        }
    }
}
