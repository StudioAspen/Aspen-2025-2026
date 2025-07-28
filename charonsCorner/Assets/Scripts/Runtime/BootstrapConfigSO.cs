using AYellowpaper.SerializedCollections;
using Eflatun.SceneReference;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "BootstrapConfig", menuName = "CharonsCorner/Bootstrap/Config")]
    public class  BootstrapConfigSO : ScriptableObject
    {
        [InfoBox("Set the initial GameState for each scene. If a scene is not listed, it defaults to Gameplay.")]
        [SerializeField, SerializedDictionary("Scene", "Initial Game State")]
        private SerializedDictionary<SceneReference, GameState> sceneInitialStates = new();

        /// <summary>
        /// Needed since SceneReferences are not hashed and can't be looked up in a dictionary.
        /// </summary>
        private Dictionary<string, GameState> sceneNameInitialStatesMap = new();

        /// <summary>
        /// Initializes the dictionary that maps scene names to their initial game states.
        /// Called in Bootstrap awake.
        /// </summary>
        public void Initialize()
        {
            sceneNameInitialStatesMap = sceneInitialStates.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value
            );
        }

        /// <summary>
        /// Gets the initial game state for a given scene from the dictionary.
        /// Returns Gameplay state if the scene is not found in the dictionary.
        /// </summary>
        public GameState GetSceneInitialState(SceneReference scene)
        {
            if (!sceneNameInitialStatesMap.ContainsKey(scene.Name))
            {
                Debug.LogWarning($"Scene '{scene.Name}' not found in BootstrapConfig. Defaulting to Gameplay state.");
                return GameState.Gameplay; // Default to Gameplay state if not found
            }

            return sceneNameInitialStatesMap[scene.Name];
        }
    }
}
