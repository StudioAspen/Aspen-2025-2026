using System.Collections.Generic;
using Febucci.TextAnimatorForUnity;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Lane : MonoBehaviour
    {
        [System.Serializable]
        public class LaneLevelColorData
        {
            public LevelDataSO LevelData;
            public Material RoofMaterial;
            public Material LaneFrameMaterial;
            public Material LaneFloorMaterial;
            public Material WallMaterial;
            public GameObject Prefab;
            public Material Skybox;
        }

        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private TypewriterComponent _typewriter;
        [SerializeField] private HubLaneSelector _laneSelector;
        [SerializeField] private MMF_Player _swapFeedback;

        [Header("Renderers")]
        [SerializeField] private MeshRenderer _roof;
        [SerializeField] private MeshRenderer _laneFrame;
        [SerializeField] private MeshRenderer _wall;

        [Header("Level Materials")]
        [SerializeField] private List<LaneLevelColorData> _levelColorSettings = new List<LaneLevelColorData>();

        private GameObject _currentLevelPrefab;
        private bool _isSelectorActive;

        private void OnEnable()
        {
            if (_laneSelector != null)
            {
                _laneSelector.OnLaneSelected.AddListener(OnLaneSelected);
                _laneSelector.OnEnter.AddListener(OnSelectorEnter);
                _laneSelector.OnLeave.AddListener(OnSelectorLeave);
                // Initialize with current lane
                UpdateTextImmediately(_laneSelector.CurrentLaneIndex);
            }
        }

        private void OnDisable()
        {
            if (_laneSelector != null)
            {
                _laneSelector.OnLaneSelected.RemoveListener(OnLaneSelected);
                _laneSelector.OnEnter.RemoveListener(OnSelectorEnter);
                _laneSelector.OnLeave.RemoveListener(OnSelectorLeave);
            }
        }

        private void OnSelectorEnter()
        {
            _isSelectorActive = true;
            UpdateLaneColors(_laneSelector.CurrentLaneIndex);
        }

        private void OnSelectorLeave()
        {
            _isSelectorActive = false;
            if (_currentLevelPrefab != null)
            {
                Destroy(_currentLevelPrefab);
                _currentLevelPrefab = null;
            }
        }

        private void OnLaneSelected(int index)
        {
            if (_displayText == null || _laneSelector == null) return;
            if (index < 0 || index >= _laneSelector.LaneData.Count) return;

            string newName = _laneSelector.LaneData[index].LevelTitle;

            UpdateLaneColors(index);

            if (_typewriter != null)
            {
                _typewriter.ShowText(newName);
            }
            else
            {
                _displayText.text = newName;
            }

            if (_swapFeedback != null)
            {
                _swapFeedback.PlayFeedbacks();
            }
        }

        private void UpdateLaneColors(int index)
        {
            if (_laneSelector == null || index < 0 || index >= _laneSelector.LaneData.Count) return;

            LevelDataSO currentLevelData = _laneSelector.LaneData[index];
            LaneLevelColorData colorData = _levelColorSettings.Find(x => x.LevelData == currentLevelData);

            // Destroy current prefab if it exists
            if (_currentLevelPrefab != null)
            {
                Destroy(_currentLevelPrefab);
                _currentLevelPrefab = null;
            }

            if (colorData == null) return;

            SetRendererMaterial(_roof, colorData.RoofMaterial, 1);
            SetRendererMaterial(_laneFrame, colorData.LaneFrameMaterial);
            SetRendererMaterial(_laneFrame, colorData.LaneFloorMaterial, 1);
            SetRendererMaterial(_wall, colorData.WallMaterial);

            // Instantiate new prefab if it exists and selector is active
            if (_isSelectorActive)
            {
                if (colorData.Prefab != null)
                {
                    _currentLevelPrefab = Instantiate(colorData.Prefab, transform);
                }

                if (colorData.Skybox != null)
                {
                    RenderSettings.skybox = colorData.Skybox;
                }
            }
        }

        private void SetRendererMaterial(MeshRenderer renderer, Material material, int materialIndex = 0)
        {
            if (renderer == null || material == null) return;

            Material[] materials = renderer.sharedMaterials;
            if (materialIndex < 0 || materialIndex >= materials.Length) return;

            materials[materialIndex] = material;
            renderer.sharedMaterials = materials;
        }

        private void UpdateTextImmediately(int index)
        {
            if (_displayText == null || _laneSelector == null) return;
            
            if (index >= 0 && index < _laneSelector.LaneData.Count)
            {
                UpdateLaneColors(index);
                string newName = _laneSelector.LaneData[index].LevelTitle;
                if (_typewriter != null)
                {
                    _typewriter.ShowText(newName);
                }
                else
                {
                    _displayText.text = newName;
                }
            }
        }
    }
}
