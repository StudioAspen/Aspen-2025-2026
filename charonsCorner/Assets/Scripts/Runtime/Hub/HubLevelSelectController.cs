using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using CharonsCorner.Runtime;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

/// <summary>
/// Controls the level select process to be used by the UI.
/// </summary>
public class HubLevelSelectController : MonoBehaviour
{
    public event Action<LevelDataSO> OnLevelSelectOpen;
    public event Action OnLevelSelectClose;
    public event Action OnLevelStarted;

    //Editor references
    [Header("References")]
    [SerializeField, ReadOnly] private LevelDataSO _currentLevelData;

    [SerializeField, ReadOnly] private bool _isOpen;

    public bool IsOpen => _isOpen;

    /// <summary>
    /// Called by the trigger when the player approaches a level entrance.
    /// </summary>
    public void OpenLevelSelect(LevelDataSO data)
    {
        if (_isOpen) return;

        _currentLevelData = data;
        _isOpen = true;
        OnLevelSelectOpen?.Invoke(_currentLevelData);

    }

    /// <summary>
    /// Called by the popup’s Close button or automatically when player leaves range.
    /// </summary>
    [Button("Close")]
    public void CloseLevelSelect()
    {
        if (!_isOpen) return;

        _currentLevelData = null;
        _isOpen = false;
        OnLevelSelectClose?.Invoke();
        Debug.Log("[HubLevelSelectController] Closed level select.");
    }

    /// <summary>
    /// Starts the selected level via GameManager.
    /// </summary>
    [Button("Start Level")]
    public void StartLevel()
    {
        if (_currentLevelData == null)
        {
            Debug.LogWarning("[HubLevelSelectController] Missing level data to start");  
            return;
        }

        Debug.Log($"[HubLevelSelectController] Starting level: {_currentLevelData.LevelScene}");
        OnLevelStarted?.Invoke();

        //Open level scene
        GameManager.Instance.SwitchScenes(_currentLevelData.LevelScene, GameState.Gameplay).Forget();
    }
}
