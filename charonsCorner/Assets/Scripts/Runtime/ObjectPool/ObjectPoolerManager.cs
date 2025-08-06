using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ObjectPoolerManager : Singleton<ObjectPoolerManager>
    {
        [Header("Settings")]
        [SerializeField] private int defaultCapacity = 100;
        [SerializeField] private int defaultMaxSize = 150;

        /// <summary>
        /// Key: Poolable Prefab
        /// Value: Associated ObjectPooler
        /// </summary>
        private Dictionary<PoolableMonoBehaviour, ObjectPooler> objectPoolerDictionary = new();

        private protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Spawns a prefab by using an existing pooler or creating a new one if it doesnt exist.
        /// </summary>
        public T SpawnPooledObject<T>(PoolableMonoBehaviour newPrefab, Vector3? position = null, Transform parent = null) where T : Component
        {
            if (objectPoolerDictionary.ContainsKey(newPrefab))
            {
                ObjectPooler pooler = objectPoolerDictionary[newPrefab];
                return pooler.SpawnObject<T>(position, parent);
            }

            GameObject newPoolerGameObject = new GameObject($"{newPrefab.name}Pooler");
            newPoolerGameObject.transform.SetParent(transform);
            ObjectPooler newPooler = newPoolerGameObject.AddComponent<ObjectPooler>();
            newPooler.Init(newPrefab, defaultCapacity, defaultMaxSize);
            objectPoolerDictionary.Add(newPrefab, newPooler);

            return newPooler.SpawnObject<T>(position, parent);
        }
    }
}
