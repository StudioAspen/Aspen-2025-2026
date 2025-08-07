using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PoolableObject : MonoBehaviour
    {
        public ObjectPooler ObjectPooler { get; private set; }

        public void SetObjectPooler(ObjectPooler pooler)
        {
            ObjectPooler = pooler;
        }
    }

    public static class PoolableObjectExtensions
    {
        public static void ReturnToPool(this GameObject gameObject)
        {
            PoolableObject poolableObject = gameObject.GetComponent<PoolableObject>();
            if(poolableObject == null)
            {
                Debug.LogWarning($"GameObject does not have a PoolableObject component or ObjectPooler is not set." +
                    $"Use GameObject.Destroy() instead.");
                return;
            }
            poolableObject.ObjectPooler.ReleaseObject(poolableObject);
        }
    }
}
