using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class HubLaneSelector : MonoBehaviour
    {
        [field: SerializeField] public List<LevelDataSO> LaneData { get; private set; } = new List<LevelDataSO>();
        [ShowInInspector, ReadOnly] public int CurrentLaneIndex { get; private set; }
        
        private InputManager _input;

        public UnityEvent OnLeave = new();
        public UnityEvent<int> OnLaneSelected = new();
        public UnityEvent<LevelDataSO> OnLaneInteracted = new();
        public UnityEvent<int> OnLaneInteractedIndex = new();

        private void Awake()
        {
            _input = InputManager.Instance;
        }

        private void OnEnable()
        {
            if (_input)
            {
                _input.Move += OnMove;
                _input.Interact += OnInteract;
            }
        }

        private void OnDisable()
        {
            if (_input)
            {
                _input.Move -= OnMove;
                _input.Interact -= OnInteract;
            }
        }

        private void OnMove(Vector2 direction)
        {
            if (direction.y < 0)
            {
                OnLeave.Invoke();
                return;
            }
            
            if (direction.x < 0)
                SelectPreviousLane();   
            else if (direction.x > 0)
                SelectNextLane();
        }

        private void OnInteract()
        {
            LevelDataSO selectedLaneData = LaneData[CurrentLaneIndex];
            
            int world1CurrentChapterFlagIndex = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
            if (selectedLaneData.WorldFlagIndex > world1CurrentChapterFlagIndex)
            {
                Debug.LogWarning($"Current progression is at {world1CurrentChapterFlagIndex}, cannot open level select for {selectedLaneData.LevelTitle} (Flag Index: {selectedLaneData.WorldFlagIndex})");
                return;
            }
            
            OnLaneInteracted?.Invoke(selectedLaneData);
            OnLaneInteractedIndex?.Invoke(CurrentLaneIndex);
        }

        public void SelectLane(int index)
        {
            if (index < 0 || index >= LaneData.Count)
                return;
            
            CurrentLaneIndex = index;
            OnLaneSelected.Invoke(CurrentLaneIndex);
        }

        public void SelectNextLane()
        {
            if (CurrentLaneIndex == LaneData.Count - 1)
            {
                // SelectLane(0); // Uncomment to get wrapping
                return;
            }
            
            SelectLane(CurrentLaneIndex + 1);
        }

        public void SelectPreviousLane()
        {
            if (CurrentLaneIndex == 0)
            {
                // SelectLane(_lanes.Count - 1); // Uncomment to get wrapping
                return;
            }
            
            SelectLane(CurrentLaneIndex - 1);
        }
        
        public LevelDataSO GetCurrentLevelData() => LaneData[CurrentLaneIndex];
    }
}