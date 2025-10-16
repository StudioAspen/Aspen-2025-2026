using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject item;
        [SerializeField]
        private float coolDown;
        [SerializeField]
        private GameObject spawnPoint;
        private float timer;
        private GameObject itemSpawned;
        private bool itemTaken = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            itemSpawned = Instantiate(item, spawnPoint.transform.position, Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
            if (itemTaken && Time.time > timer + coolDown)
            {
                itemTaken = false;
                itemSpawned = Instantiate(item, spawnPoint.transform.position, Quaternion.identity);
            }

            if (itemSpawned == null && !itemTaken)
            {
                timer = Time.time;
                itemTaken = true;
            }
        }
    }
}
