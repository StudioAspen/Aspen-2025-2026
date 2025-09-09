using UnityEngine;
using UnityEngine.Pool;

namespace CharonsCorner.Runtime
{
    public class ObjectPooler : MonoBehaviour
    {
        private PoolableObject _objectPrefab;

        private ObjectPool<PoolableObject> _objectPool;

        public void Init(PoolableObject objectPrefab, int capacity, int maxSize)
        {
            this._objectPrefab = objectPrefab;
            _objectPool = new ObjectPool<PoolableObject>(CreateObject, OnGetFromPool, OnReleaseToPool, OnDestroyObject, true, capacity, maxSize);
        }

        private PoolableObject CreateObject()
        {
            PoolableObject newObject = Instantiate(_objectPrefab, new Vector3(0f, 100000f, 0f), Quaternion.identity, transform);
            newObject.SetObjectPooler(this);

            return newObject;
        }

        private void OnGetFromPool(PoolableObject pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(PoolableObject pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        private void OnDestroyObject(PoolableObject pooledObject)
        {
            Destroy(pooledObject);
        }

        #region Factory
        public T SpawnObject<T>(Vector3? position = null, Transform parent = null) where T : Component
        {
            PoolableObject spawnedObject = _objectPool.Get();

            // Set position if provided, otherwise default to zero
            spawnedObject.transform.position = position ?? Vector3.zero;

            // Set parent if provided
            if (parent != null)
            {
                spawnedObject.transform.SetParent(parent);
            }

            Physics.SyncTransforms();

            T component = spawnedObject.GetComponent<T>();
            Debug.Assert(component != null, $"Prefab is missing {typeof(T)} component");

            return component;
        }

        /// <summary>
        /// Use this instead of Destroy() to return the object to the pool.
        /// </summary>
        public void ReleaseObject(PoolableObject pooledObject)
        {
            _objectPool.Release(pooledObject);
        }
        #endregion
    }
}
