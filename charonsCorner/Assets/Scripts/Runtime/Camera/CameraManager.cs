using System;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Manages Cinemachine cameras by storing the default and current camera.
    /// All cameras in all scenes should be Cinemachine cameras.
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        /// <summary>
        /// The default camera the scene starts with or fallbacks to.
        /// Assigned through the SceneDefaultCameraRegisterer component.
        /// Always wiped when changing scenes.
        /// </summary>
        [field: SerializeField, ReadOnly] public CinemachineCamera SceneDefaultCamera { get; private set; }
        /// <summary>
        /// The current camera the scene is using.
        /// Always wiped when changing scenes.
        /// </summary>
        [field: SerializeField, ReadOnly] public CinemachineCamera CurrentCamera { get; private set; }
        /// <summary>
        /// Action that is invoked when the current camera changes.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description><c>CinemachineCamera newCamera</c>: The camera that was switched to.</description></item>
        /// </list>
        /// </remarks>
        public event Action<CinemachineCamera> OnActiveCameraChanged = delegate { };

        /// <summary>
        /// CameraShaker instance that can be used to shake the current camera.
        /// </summary>
        public CameraShaker CameraShaker { get; private set; }

        private protected override void Awake()
        {
            base.Awake();

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;

            CameraShaker = new CameraShaker(this);
        }

        private protected override void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;

            CameraShaker.Dispose();
        }

        /// <summary>
        /// Called by the SceneDefaultCameraRegisterer attached to the default Cinemachine camera of the scene.
        /// Rarely needs to be called more than once per scene.
        /// </summary>
        /// <param name="camera">The camera to register as the scene's default camera.</param>
        /// <param name="changeToActiveCamera">Whether to change the current camera to the default one.</param>
        public void RegisterSceneDefaultCamera(CinemachineCamera camera, bool changeToActiveCamera = false)
        {
            SceneDefaultCamera = camera;

            if (changeToActiveCamera)
                ChangeActiveCamera(camera);
        }

        /// <summary>
        /// Changes the current camera and makes it the active one through Cinemachine.
        /// </summary>
        /// <param name="camera">The camera to switch to.</param>
        public void ChangeActiveCamera(CinemachineCamera camera)
        {
            CurrentCamera = camera;
            CurrentCamera.Prioritize();

            OnActiveCameraChanged.Invoke(CurrentCamera);
        }

        /// <summary>
        /// Switches the current camera back to the default one.
        /// </summary>
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
