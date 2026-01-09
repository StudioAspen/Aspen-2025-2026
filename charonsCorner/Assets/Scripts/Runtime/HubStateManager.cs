using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public enum HubState
    {
        TitleScreen,
        Gameplay
    }

    public class HubStateManager : MonoBehaviour
    {
        public static HubStateManager Instance { get; private set; }

        public event Action<HubState> OnStateChanged;

        [SerializeField] private HubState _currentState = HubState.TitleScreen;

        public HubState CurrentState => _currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RequestStateChange(HubState newState)
        {
            Debug.Log($"[HubStateManager] Requesting state change to: {newState}");
            if (_currentState == newState) return;

            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }

        public void SwitchToGameplay() => RequestStateChange(HubState.Gameplay);
        public void SwitchToTitleScreen() => RequestStateChange(HubState.TitleScreen);
    }
}
