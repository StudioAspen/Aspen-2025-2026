using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(HubLaneSelector))]
    public class HubLaneSelectorSpotlightController : MonoBehaviour
    {
        private HubLaneSelector _hubLaneSelector;

        [SerializeField] private List<GameObject> _spotlightObjects = new();
        
        [Header("Config")]
        [SerializeField] private int _flickerCount = 3;
        [SerializeField] private float _flickerInterval = 0.25f;

        private Coroutine _flickerCoroutine;
        
        public UnityEvent<int> OnFlickerStartedIndex = new();
        public UnityEvent<int> OnFlickerEndedIndex = new();
        public UnityEvent<LevelDataSO> OnFlickerEndedData = new();

        private void Awake()
        {
            _hubLaneSelector = GetComponent<HubLaneSelector>();

            if (_spotlightObjects.Count != _hubLaneSelector.LaneData.Count)
            {
                Debug.LogWarning($"You must have the same number of spotlight objects and lanes. The list index of the lane corresponds to the index of the spotlight object.");
            }
        }

        private void OnEnable()
        {
            _hubLaneSelector.OnLaneSelected.AddListener(OnLaneSelected);
        }

        private void OnDisable()
        {
            _hubLaneSelector.OnLaneSelected.RemoveListener(OnLaneSelected);
        }

        private void OnLaneSelected(int selectedLaneIndex)
        {
            DisableAllSpotlights();

            if (selectedLaneIndex >= _spotlightObjects.Count)
            {
                Debug.LogError($"Cannot enable spotlight {selectedLaneIndex}. You must have the same number of spotlight objects and lanes. The list index of the lane corresponds to the index of the spotlight object.");
                return;
            }
            _spotlightObjects[selectedLaneIndex].transform.parent.gameObject.SetActive(true); // Enable parent for input canvas
        }
        
        /// <summary>
        /// Disable spotlight parents to also disable input canvas.
        /// </summary>
        public void DisableAllSpotlights()
        {
            foreach (var spotlight in _spotlightObjects)
            {
                if(spotlight != null)
                    spotlight.transform.parent.gameObject.SetActive(false);
            }
        }
        
        public void FlickerLight(int lightIndex)
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
                _flickerCoroutine = null;
            }
            
            _flickerCoroutine = StartCoroutine((IEnumerator)FlickerCoroutine(lightIndex));
        }

        private IEnumerator FlickerCoroutine(int lightIndex)
        {
            OnFlickerStartedIndex?.Invoke(lightIndex);
            
            GameObject lightObject = _spotlightObjects[lightIndex];
            lightObject.SetActive(true);
            for (int i = 0; i < _flickerCount * 2; i++)
            {
                lightObject.SetActive(!lightObject.activeInHierarchy);
                yield return new WaitForSeconds(_flickerInterval / 2f);
            }
            
            _flickerCoroutine = null;
            
            OnFlickerEndedIndex?.Invoke(lightIndex);
            OnFlickerEndedData?.Invoke(_hubLaneSelector.LaneData[lightIndex]);
        }
    }
}