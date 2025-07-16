using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class EventSystemSingleton : MonoBehaviour
    {
        private static EventSystemSingleton instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
