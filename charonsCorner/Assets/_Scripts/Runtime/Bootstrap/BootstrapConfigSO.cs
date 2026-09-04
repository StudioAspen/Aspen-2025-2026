using AYellowpaper.SerializedCollections;
using Eflatun.SceneReference;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This class houses the configuration for the BootstrapLoader to load a target scene with the correct game state in the editor.
    /// Builds do not use this script. Required because force loading into a specific scene needs game state context to work properly.
    /// For example, when you load into a test level, you want to be in the Gameplay state instead of the Title state. Scenes not added to this config
    /// will default to Gameplay state.
    /// </summary>
    [CreateAssetMenu(fileName = "BootstrapConfig", menuName = "CharonsCorner/Bootstrap/Config")]
    public class BootstrapConfigSO : ScriptableObject
    {
        [InfoBox("Set the initial GameState for each scene. If a scene is not listed, it defaults to Gameplay.")]
        [SerializeField, SerializedDictionary("Scene", "Initial Game State"), DrawWithUnity]
        private SerializedDictionary<SceneReference, GameState> _sceneInitialStates = new();

        /// <summary>
        /// Needed since SceneReferences are not hashed and can't be looked up in a dictionary.
        /// </summary>
        private Dictionary<string, GameState> _sceneNameInitialStatesMap = new();

        /// <summary>
        /// Initializes a helper dictionary that maps scene names to their initial game states.
        /// We need to do this because SceneReference types are not hashable while strings are.
        /// Called once in BootstrapLoader's Awake method.
        /// </summary>
        public void Initialize()
        {
            _sceneNameInitialStatesMap = _sceneInitialStates.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value
            );
        }

        /// <summary>
        /// Gets the initial game state for a given scene from the dictionary.
        /// </summary>
        /// <param name="scene">The scene used to look up the matching game state.</param>
        /// <returns>The game state that maps to the scene, or Gameplay state if the scene is not found.</returns>
        public GameState GetSceneInitialState(SceneReference scene)
        {
            if (!_sceneNameInitialStatesMap.ContainsKey(scene.Name))
            {
                Debug.LogWarning($"Scene '{scene.Name}' not found in BootstrapConfig. Defaulting to Gameplay state.");
                return GameState.Gameplay; // Default to Gameplay state if not found
            }

            return _sceneNameInitialStatesMap[scene.Name];
        }
    }
}
