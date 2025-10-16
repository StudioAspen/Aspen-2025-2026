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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            itemSpawned = Instantiate(item, spawnPoint.transform.position, Quaternion.identity);
        }

        // Update is called once per frame
        void Update()
        {
            if (itemSpawned != null && Time.time > timer + coolDown)
            {
                itemSpawned = Instantiate(item, spawnPoint.transform.position, Quaternion.identity);
            }

            if (itemSpawned == null)
            {
                timer = Time.time;
            }
        }
    }
}
