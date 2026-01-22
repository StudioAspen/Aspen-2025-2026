using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class HubLaneSelector : MonoBehaviour
    {
        [System.Serializable]
        public class LaneData
        {
            public GameObject SpotlightObject;
            public LevelDataSO LevelData;
        }
        
        [SerializeField] private List<LaneData> _lanes = new List<LaneData>();
        [ShowInInspector, ReadOnly] private int _currentLaneIndex;
        
        private InputManager _input;

        public UnityEvent<LevelDataSO> OnLaneInteracted = new();

        private void Awake()
        {
            _input = InputManager.Instance;
            
            _currentLaneIndex = -1;
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
            if (direction.x < 0)
            {
                SelectPreviousLane();   
            }
            else if (direction.x > 0)
            {
                SelectNextLane();
            }
        }

        private void OnInteract()
        {
            LaneData selectedLane = _lanes[_currentLaneIndex];
            OnLaneInteracted?.Invoke(selectedLane.LevelData);
        }

        public void SelectLane(int index)
        {
            if(_currentLaneIndex == index)
                return;

            if (index < 0 || index >= _lanes.Count)
                return;
            
            _currentLaneIndex = index;
            OnLaneSelected(GetCurrentLaneData());
        }

        public void SelectNextLane()
        {
            if (_currentLaneIndex == _lanes.Count - 1)
            {
                // SelectLane(0); // Uncomment to get wrapping
                return;
            }
            
            SelectLane(_currentLaneIndex + 1);
        }

        public void SelectPreviousLane()
        {
            if (_currentLaneIndex == 0)
            {
                // SelectLane(_lanes.Count - 1); // Uncomment to get wrapping
                return;
            }
            
            SelectLane(_currentLaneIndex - 1);
        }
        
        public LaneData GetCurrentLaneData() => _lanes[_currentLaneIndex];

        public void DisableAllSpotlights()
        {
            foreach (var laneData in _lanes)
            {
                if(laneData.SpotlightObject != null)
                    laneData.SpotlightObject.SetActive(false);
            }
        }
        
        private void OnLaneSelected(LaneData laneData)
        {
            DisableAllSpotlights();
            laneData.SpotlightObject.SetActive(true);
        }
    }
}