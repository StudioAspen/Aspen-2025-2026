using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ProjectileTimer : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 5f;
        void Start()
        {
            Destroy(gameObject, _lifeTime);
        }
    }
}
