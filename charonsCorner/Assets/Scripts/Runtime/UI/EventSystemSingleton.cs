using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
            DestroyOtherEventSystems();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
        }

        private void SceneManager_ActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            DestroyOtherEventSystems();
        }

        private void DestroyOtherEventSystems()
        {
            // Destroy all other EventSystems in the scene except this one
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            foreach (EventSystem eventSystem in eventSystems)
            {
                if (eventSystem == null)
                    continue;

                if (eventSystem.gameObject != gameObject)
                {
                    Debug.LogWarning($"Destroying duplicate EventSystem: {eventSystem.gameObject.name}");
                    Destroy(eventSystem.gameObject);
                }
            }
        }
    }
}
