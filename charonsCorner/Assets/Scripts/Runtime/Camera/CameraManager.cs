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

        [field: SerializeField, ReadOnly] public CinemachineCamera SceneDefaultCamera { get; private set; }
        [field: SerializeField, ReadOnly] public CinemachineCamera CurrentCamera { get; private set; }
        public event Action<CinemachineCamera> OnActiveCameraChanged = delegate { };

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

            CameraShaker.Dispose();
        }

        public void RegisterSceneDefaultCamera(CinemachineCamera camera, bool changeToActiveCamera = false)
        {
            SceneDefaultCamera = camera;

            if (changeToActiveCamera)
                ChangeActiveCamera(camera);
        }

        public void ChangeActiveCamera(CinemachineCamera camera)
        {
            CurrentCamera = camera;
            CurrentCamera.Prioritize();

            OnActiveCameraChanged.Invoke(CurrentCamera);
        }

        public void ResetActiveCamera()
        {
            if (SceneDefaultCamera == null)
            {
                Debug.LogWarning("No default camera registered. Cannot reset active camera.");
                return;
            }

            ChangeActiveCamera(SceneDefaultCamera);
        }

        private void Update()
        {
            CameraShaker.Update();
        }

        private void SceneManager_ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            SceneDefaultCamera = null;
            CurrentCamera = null;
        }
    }
}
