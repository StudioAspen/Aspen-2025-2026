using UnityEngine;

public class HubLevelSelectTrigger : MonoBehaviour
{
    [SerializeField] private HubLevelSelectController _controller;
    [SerializeField] private LevelDataSO _currentLevelData;

    /// <summary>
    /// Calls controller's OpenLevelSelect() method when the player interacts with this portal (uses TouchInteractable)
    /// </summary>
    public void TriggerOpenLevelSelect()
    {
        //Error Handling
        if (_controller == null)
        {
            Debug.LogError("[HubLevelSelectTrigger] Missing controller reference on " + gameObject.name);
            return;
        }

        if (_currentLevelData == null)
        {
            Debug.LogError("[HubLevelSelectTrigger] Missing LevelData reference on " + gameObject.name);
            return;
        }

        //Call controller method
        _controller.OpenLevelSelect(_currentLevelData);
    }
}
