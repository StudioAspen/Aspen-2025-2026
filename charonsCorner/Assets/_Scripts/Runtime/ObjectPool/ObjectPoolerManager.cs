using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ObjectPoolerManager : Singleton<ObjectPoolerManager>
    {
        [Header("Settings")]
        [SerializeField] private int _defaultCapacity = 100;
        [SerializeField] private int _defaultMaxSize = 150;

        /// <summary>
        /// Key: Poolable Prefab
        /// Value: Associated ObjectPooler
        /// </summary>
        private Dictionary<PoolableObject, ObjectPooler> _objectPoolerDictionary = new();

        private protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Spawns a prefab by using an existing pooler or creating a new one if it doesnt exist.
        /// </summary>
        public T SpawnPooledObject<T>(PoolableObject newPrefab, Vector3? position = null, Transform parent = null) where T : MonoBehaviour
        {
            if (_objectPoolerDictionary.ContainsKey(newPrefab))
            {
                ObjectPooler pooler = _objectPoolerDictionary[newPrefab];
                return pooler.SpawnObject<T>(position, parent);
            }

            GameObject newPoolerGameObject = new GameObject($"{newPrefab.name}Pooler");
            newPoolerGameObject.transform.SetParent(transform);
            ObjectPooler newPooler = newPoolerGameObject.AddComponent<ObjectPooler>();
            newPooler.Init(newPrefab, _defaultCapacity, _defaultMaxSize);
            _objectPoolerDictionary.Add(newPrefab, newPooler);

            return newPooler.SpawnObject<T>(position, parent);
        }
    }
}
