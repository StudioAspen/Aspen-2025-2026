using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class SceneDependentUI : MonoBehaviour
    {
        [InfoBox("This UI element will be hidden in the scenes specified in the list below.")]
        [SerializeField] private List<SceneReference> _scenesToHideFrom = new();
        private HashSet<string> _lookUpHash = new();

        [InfoBox("Check this if you want to invert the visibility logic. The UI will be visible only in the scenes specified in the list above, and hidden in all other scenes.")]
        [SerializeField] private bool _invertVisibility = false;

        private void Awake()
        {
            // Cache the scenes to hide from in a hash set for quick lookups
            foreach(SceneReference scene in _scenesToHideFrom)
            {
                if (scene == null)
                    continue;
                _lookUpHash.Add(scene.Name);
            }
        }

        private void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
            SceneManager_ActiveSceneChanged(default, GameManager.Instance.GetCurrentScene().LoadedScene);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
        }

        private void SceneManager_ActiveSceneChanged(Scene previousScene, Scene newScene)
        {
            // Hides the UI element if the scene is part of the scenesToHideFrom list
            bool shouldShow = !_lookUpHash.Contains(newScene.name);
            if (_invertVisibility)
                shouldShow = !shouldShow;
            gameObject.SetActive(shouldShow);
        }
    }
}
