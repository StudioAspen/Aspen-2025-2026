using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PoolableMonoBehaviour : MonoBehaviour
    {
        private protected ObjectPooler objectPooler { get; private set; }

        public void SetObjectPooler(ObjectPooler pooler) => objectPooler = pooler;
    }
}
