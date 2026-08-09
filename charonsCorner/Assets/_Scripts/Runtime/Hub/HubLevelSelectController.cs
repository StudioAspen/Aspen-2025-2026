using System;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using MoreMountains.Feedbacks;
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
        public UnityEvent OnLevelStarted = new();

        //Editor references
        [Header("References")]
        [SerializeField] private InputInteraction _inputInteraction;
        [SerializeField] private SceneReference _flashbackScene;
        [SerializeField] private MMF_Player _bowlingStarter;
        [SerializeField] private float _unfadeDuration = 2f;
        [SerializeField, ReadOnly] private LevelDataSO _currentLevelData;

        [SerializeField, ReadOnly] private bool _isOpen;
        [SerializeField, ReadOnly] private bool _isStartingLevel;
        
        public bool IsOpen => _isOpen;
        
        public void OpenLevelSelect(LevelDataSO data)
        {
            if (_isOpen || _isStartingLevel) 
                return;

            _currentLevelData = data;
            _isOpen = true;

            if (_inputInteraction != null)
                _inputInteraction.Appear();

            OnLevelSelectOpen?.Invoke(_currentLevelData);
        }
        
        [Button("Close")]
        public void CloseLevelSelect()
        {
            if (!_isOpen || _isStartingLevel) 
                return;

            _currentLevelData = null;
            _isOpen = false;

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            OnLevelSelectClose?.Invoke();
            // Debug.Log("[HubLevelSelectController] Closed level select.");
        }

        /// <summary>
        /// Starts the selected level via GameManager.
        /// </summary>
        [Button("Start Level")]
        public void StartLevel()
        {
            StartLevelAsync().Forget();
        }

        private async UniTask StartLevelAsync()
        {
            if (_currentLevelData == null)
            {
                Debug.LogWarning("[HubLevelSelectController] Missing level data to start");
                return;
            }

            if (_flashbackScene == null)
            {
                Debug.LogWarning("[HubLevelSelectController] Flashback scene is not assigned.");
                return;
            }

            if (_isStartingLevel)
                return;

            _isStartingLevel = true;

            if (_inputInteraction != null)
                _inputInteraction.Disappear();

            // Fade to black before starting the sequence
            if (LoadingCanvas.Instance != null)
            {
                await LoadingCanvas.Instance.FadeIn();
            }

            Debug.Log($"[HubLevelSelectController] Starting level sequence for: {_currentLevelData.LevelTitle}");

            if (_bowlingStarter != null)
            {
                bool skipped = false;
                Action onInteract = () => skipped = true;

                if (InputManager.Instance != null)
                {
                    InputManager.Instance.Interact += onInteract;
                }

                _bowlingStarter.PlayFeedbacks();

                // Start unfading shortly after feedback starts to reveal the cutscene
                if (LoadingCanvas.Instance != null)
                {
                    // Using a dummy panel or null to just fade out
                    LoadingCanvas.Instance.FadeOut(null).Forget();
                }

                // Wait until feedback is done or player interacts
                await UniTask.WaitUntil(() => !_bowlingStarter.IsPlaying || skipped);

                if (InputManager.Instance != null)
                {
                    InputManager.Instance.Interact -= onInteract;
                }
            }

            Debug.Log($"[HubLevelSelectController] Starting flashback for level: {_currentLevelData.LevelTitle}");

            // Set the pending dialogue for the flashback scene
            FlashbackTrigger.SetPendingDialogue(_currentLevelData.FlashbackDialogue);

            // Open flashback scene
            if (GameManager.Instance != null)
            {
                await GameManager.Instance.SwitchScenes(_flashbackScene, GameState.Cutscene);
            }
            else
            {
                Debug.LogError("[HubLevelSelectController] GameManager.Instance is null! Cannot switch scenes.");
            }

            OnLevelStarted?.Invoke();
            _isStartingLevel = false;
        }
    }
}