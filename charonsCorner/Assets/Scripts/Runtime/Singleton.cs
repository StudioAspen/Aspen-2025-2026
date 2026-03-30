using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _applicationIsQuitting;

        /// <summary>
        /// Returns the singleton instance if one exists in the scene.
        /// Returns null if no instance is present.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                    return null;
                }

                if (_instance == null)
                {
                    // Try to find an existing instance in the scene
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        // Do NOT create a new object
                        Debug.LogWarning($"[Singleton] No instance of {typeof(T)} found in the scene. Returning null.");
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Override in derived classes and call base.Awake() if needed.
        /// </summary>
        private protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this as T;
            /*DontDestroyOnLoad(gameObject);*/
        }

        private protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
