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
                
                // Explicitly handle the initial state without it being a transition from itself
                ExecuteTransition(GameState.Loading, _previousState); 
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
            if (_previousState == newState) return;

            ExecuteTransition(_previousState, newState);

            _previousState = newState;
        }

        private void ExecuteTransition(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Title:
                    PlayFeedback(_sequenceTitle);
                    break;

                case GameState.Gameplay:
                    HandleGameplayEntry(from);
                    break;
            }
        }

        private void HandleGameplayEntry(GameState from)
        {
            // We don't play any sequences when returning from pause
            if (from == GameState.Paused) return;

            if (from == GameState.Title)
            {
                PlayFeedback(_sequenceExitTitle);
            }
            else
            {
                PlayFeedback(_sequenceToGameplay);
            }
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
