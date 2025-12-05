using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TomatoProjectile : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 5f;
        void Start()
        {
            Destroy(gameObject, _lifeTime);
        }
    }
}
