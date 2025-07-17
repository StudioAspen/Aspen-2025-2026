using Eflatun.SceneReference;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
        }

        private void Start()
        {
            GameManager.Instance.OnSceneChanged += GameManager_OnSceneChanged;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnSceneChanged -= GameManager_OnSceneChanged;
        }

        private void GameManager_OnSceneChanged(SceneReference scene)
        {
            // Destroy all other EventSystems in the scene except this one
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach(EventSystem eventSystem in eventSystems)
            {
                if(eventSystem == null) 
                    continue;

                if (eventSystem.gameObject != gameObject)
                    Destroy(eventSystem.gameObject);
            }
        }
    }
}
