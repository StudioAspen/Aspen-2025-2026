using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Controls the level select process to be used by the UI.
    /// </summary>
    public class HubLevelSelectController : MonoBehaviour
    {
        public UnityEvent<LevelDataSO> OnLevelSelectOpen = new();
        public UnityEvent OnLevelSelectClose = new();

        //Editor references
        [Header("References")]
        [SerializeField, ReadOnly] private LevelDataSO _currentLevelData;

        [SerializeField, ReadOnly] private bool _isOpen;

        public bool IsOpen => _isOpen;
        
        public void OpenLevelSelect(LevelDataSO data)
        {
            if (_isOpen) 
                return;

            _currentLevelData = data;
            _isOpen = true;
            OnLevelSelectOpen?.Invoke(_currentLevelData);
        }
        
        [Button("Close")]
        public void CloseLevelSelect()
        {
            if (!_isOpen) 
                return;

            _currentLevelData = null;
            _isOpen = false;
            OnLevelSelectClose?.Invoke();
            // Debug.Log("[HubLevelSelectController] Closed level select.");
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

            //Open level scene
            GameManager.Instance.SwitchScenes(_currentLevelData.LevelScene, GameState.Gameplay).Forget();
        }
    }
}