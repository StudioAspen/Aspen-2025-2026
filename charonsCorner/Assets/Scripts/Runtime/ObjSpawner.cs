using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ObjSpawner : MonoBehaviour
    {
        public GameObject obj;
        int randNum;
        public Transform spawnDest1, spawnDest2, spawnDest3, spawnDest4;
        public bool spawningbool = true;
        public float spawnTime;

        void Start()
        {
            StartCoroutine(spawning());
        }
        IEnumerator spawning()
        {
            while (spawningbool == true)
            {
                yield return new WaitForSeconds(spawnTime);
                randNum = Random.Range(0, 1);
                if (randNum == 0)
                {
                    Instantiate(obj, spawnDest1.position, spawnDest1.rotation);
                }
                if (randNum == 1)
                {
                    Instantiate(obj, spawnDest2.position, spawnDest2.rotation);
                }
                if (randNum == 2)
                {
                    Instantiate(obj, spawnDest1.position, spawnDest1.rotation);
                }
                if (randNum == 3)
                {
                    Instantiate(obj, spawnDest2.position, spawnDest2.rotation);
                }
                if (randNum == 4)
                {
                    Instantiate(obj, spawnDest2.position, spawnDest2.rotation);
                }
            }
        }
    }
}

