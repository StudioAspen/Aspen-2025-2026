using UnityEngine;
using UnityEngine.Pool;

namespace CharonsCorner.Runtime
{
    public class ObjectPooler : MonoBehaviour
    {
        private PoolableMonoBehaviour objectPrefab;

        private ObjectPool<PoolableMonoBehaviour> objectPool;

        public void Init(PoolableMonoBehaviour objectPrefab, int capacity, int maxSize)
        {
            this.objectPrefab = objectPrefab;
            objectPool = new ObjectPool<PoolableMonoBehaviour>(CreateObject, OnGetFromPool, OnReleaseToPool, OnDestroyObject, true, capacity, maxSize);
        }

        private PoolableMonoBehaviour CreateObject()
        {
            PoolableMonoBehaviour o = Instantiate(objectPrefab, new Vector3(0f, 100000f, 0f), Quaternion.identity, transform);
            o.SetObjectPooler(this);

            return o;
        }

        private void OnGetFromPool(PoolableMonoBehaviour pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        private void OnReleaseToPool(PoolableMonoBehaviour pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        private void OnDestroyObject(PoolableMonoBehaviour pooledObject)
        {
            Destroy(pooledObject);
        }

        #region Factory
        public T SpawnObject<T>(Vector3? position = null, Transform parent = null) where T : Component
        {
            PoolableMonoBehaviour spawnedObject = objectPool.Get();

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
        public void ReleaseObject(PoolableMonoBehaviour pooledObject)
        {
            objectPool.Release(pooledObject);
        }
        #endregion
    }
}
