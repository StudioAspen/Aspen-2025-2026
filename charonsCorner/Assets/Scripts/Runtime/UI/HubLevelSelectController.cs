using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using CharonsCorner.Runtime;
using Cysharp.Threading.Tasks;

public class HubLevelSelectController : MonoBehaviour
{
    public event Action OnLevelSelectOpen;
    public event Action OnLevelSelectClose;
    public event Action<LevelDataSO> OnLevelStarted;

    //Editor references
    [Header("References")]
    [SerializeField, ReadOnly] private LevelDataSO _currentLevelData;

    private bool _isOpen;

    public bool IsOpen => _isOpen;

    /// <summary>
    /// Called by the trigger when the player approaches a level entrance.
    /// </summary>
    [Button("Open")]
    public void OpenLevelSelect(LevelDataSO data)
    {
        if (_isOpen) return;

        _isOpen = true;
        OnLevelSelectOpen?.Invoke();

    }

    /// <summary>
    /// Called by the popup’s Close button or automatically when player leaves range.
    /// </summary>
    [Button("Close")]
    public void CloseLevelSelect()
    {
        if (!_isOpen) return;

        _isOpen = false;
        OnLevelSelectClose?.Invoke();
        Debug.Log("[HubLevelSelectController] Closed level select.");
    }

    /// <summary>
    /// Starts the selected level via GameManager.
    /// </summary>
    [Button("Start")]
    public void StartLevel()
    {
        if (_currentLevelData == null) return;

        Debug.Log($"[HubLevelSelectController] Starting level: {_currentLevelData.LevelScene}");
        OnLevelStarted?.Invoke(_currentLevelData);

        //Open level scene
        GameManager.Instance.SwitchScenes(_currentLevelData.LevelScene, GameState.Gameplay).Forget();
    }
}
