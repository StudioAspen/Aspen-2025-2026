using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks.Triggers;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [field: SerializeField, ReadOnly] public CinemachineCamera CurrentCamera { get; private set; }
        public event Action<CinemachineCamera> OnActiveCameraChanged = delegate { };

        public enum CameraType
        {
            Player,
            Dialogue,
        }

        [field: SerializeField, SerializedDictionary("Camera Type", "Camera")]
        public SerializedDictionary<CameraType, CinemachineCamera> RegisteredCameras { get; private set; } = new();

        public CameraShaker CameraShaker { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;

            CameraShaker = new CameraShaker(this);
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
        }

        public void RegisterCamera(CameraType type, CinemachineCamera camera, bool changeToActive = false)
        {
            if (!RegisteredCameras.ContainsKey(type))
                RegisteredCameras.Add(type, camera);
            else
                RegisteredCameras[type] = camera;

            if (changeToActive)
                ChangeActiveCamera(type);
        }

        public void ChangeActiveCamera(CameraType cameraType)
        {
            if (!RegisteredCameras.ContainsKey(cameraType))
            {
                Debug.LogWarning($"Camera type {cameraType} is not registered. Failed to change active camera.");
                return;
            }

            CurrentCamera = RegisteredCameras[cameraType];
            CurrentCamera.Prioritize();

            OnActiveCameraChanged.Invoke(CurrentCamera);
        }

        private void Update()
        {
            CameraShaker.Update();
        }

        private void SceneManager_ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            RegisteredCameras.Clear();
            CurrentCamera = null;
        }
    }
}
