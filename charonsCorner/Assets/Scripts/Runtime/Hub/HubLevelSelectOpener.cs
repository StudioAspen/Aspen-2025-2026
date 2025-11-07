using NaughtyAttributes;
using UnityEngine;

/// <summary>
/// Used to open level select with custom level data
/// </summary>
public class HubLevelSelectOpener : MonoBehaviour
{
    [SerializeField, Required] private HubLevelSelectController _controller;
    [SerializeField, Required] private LevelDataSO _levelData;

    /// <summary>
    /// Calls controller's OpenLevelSelect() method when the player interacts with this portal (uses TouchInteractable)
    /// </summary>
    [Button("Trigger Level Select")]
    public void TriggerOpenLevelSelect()
    {
        //Error Handling
        if (_controller == null)
        {
            Debug.LogError($"[HubLevelSelectTrigger] Missing controller reference on {gameObject.name}");
            return;
        }

        if (_levelData == null)
        {
            Debug.LogError($"[HubLevelSelectTrigger] Missing LevelData reference on {gameObject.name}");
            return;
        }

        //Call controller method
        _controller.OpenLevelSelect(_levelData);
    }
}
