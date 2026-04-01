using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    public class EventSystemSingleton : Singleton<EventSystemSingleton>
    {
        private protected override void Awake()
        {
            base.Awake();

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
            DestroyOtherEventSystems();
        }

        private protected override void OnDestroy()
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
