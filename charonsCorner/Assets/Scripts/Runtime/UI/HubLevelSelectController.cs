using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class HubLevelSelectController : MonoBehaviour
{
    public event Action OnLevelSelectOpen;
    public event Action OnLevelSelectClose;
    public event Action OnLevelStarted;

    //Editor references
    [Header("References")]
    [SerializeField] private GameObject levelSelectUIPrefab; // Prefab for the popup UI
    [SerializeField] private Transform uiSpawnPoint;         // Where to show it (optional)
    [SerializeField] private LevelDataSO currentLevelData;

    private GameObject activeUI;
    private bool isOpen;

    public bool IsOpen => isOpen;

    /// <summary>
    /// Called by the trigger when the player approaches a level entrance.
    /// </summary>
    public void OpenLevelSelect(LevelDataSO data)
    {
        //TODO: Implement
    }

    /// <summary>
    /// Called by the popup’s Close button or automatically when player leaves range.
    /// </summary>
    public void CloseLevelSelect()
    {
        if (!isOpen) return;

        //Destroy UI instance
        isOpen = false;
        if (activeUI != null)
        {
            Destroy(activeUI);
            activeUI = null;
        }

        OnLevelSelectClose?.Invoke();
        Debug.Log("[HubLevelSelectController] Closed level select.");
    }

    /// <summary>
    /// Starts the selected level via GameManager.
    /// </summary>
    public void StartLevel()
    {
        if (currentLevelData == null) return;

        Debug.Log($"[HubLevelSelectController] Starting level: {currentLevelData.levelScene}");
        OnLevelStarted?.Invoke();

        //Open level scene
        SceneManager.LoadScene(currentLevelData.levelTitle);
    }
}
