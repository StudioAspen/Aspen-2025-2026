using System;
using CharonsCorner.Runtime;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubSequenceController : MonoBehaviour
    {
        [Header("Feedbacks")]
        [SerializeField, Required] private MMF_Player _sequenceTitle;
        [SerializeField, Required] private MMF_Player _sequenceExitTitle;
        [SerializeField, Required] private MMF_Player _sequenceToGameplay;
        private GameState _previousState;

        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

                _previousState = GameManager.Instance.CurrentGameState;
                HandleGameStateChanged(GameManager.Instance.CurrentGameState);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Title:
                    PlayFeedback(_sequenceTitle);
                    break;
                case GameState.Gameplay:
                    if (_previousState == GameState.Paused)
                        break; // returning from pause, do nothing
                    if (_previousState == GameState.Title)
                        PlayFeedback(_sequenceExitTitle);
                    else
                        PlayFeedback(_sequenceToGameplay);
                    break;
            }

            _previousState = newState;
        }

        private void PlayFeedback(MMF_Player feedback)
        {
            if (feedback != null)
            {
                feedback.Initialization();
                feedback.PlayFeedbacks();
            }
        }
    }
}
