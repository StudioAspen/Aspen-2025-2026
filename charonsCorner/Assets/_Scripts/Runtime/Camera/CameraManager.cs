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

        private int _maxPriority = 10;

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
            Debug.Log($"[CameraManager] Registering Scene Default Camera: {camera.name}, changeToActiveCamera: {changeToActiveCamera}");
            SceneDefaultCamera = camera;
            
            _maxPriority = Math.Max(_maxPriority, camera.Priority.Value);

            if (changeToActiveCamera)
                ChangeActiveCamera(camera, CinemachineBlendDefinition.Styles.Linear, 0f);
        }

        /// <summary>
        /// Changes the current camera and makes it the active one through Cinemachine.
        /// </summary>
        /// <param name="camera">The camera to switch to.</param>
        public void ChangeActiveCamera(CinemachineCamera camera, CinemachineBlendDefinition.Styles? blendType = null, float blendDuration = 0.5f)
        {
            if (camera == null)
            {
                Debug.LogWarning("[CameraManager] ChangeActiveCamera called with null camera.");
                return;
            }

            Debug.Log($"[CameraManager] Changing Active Camera to: {camera.name}. Current: {(CurrentCamera != null ? CurrentCamera.name : "None")}. Blend: {(blendType.HasValue ? blendType.Value.ToString() : "Default")}, Duration: {blendDuration}");

            CurrentCamera = camera;

            // Change blend type optionally
            if (blendType.HasValue)
            {
                for (int i = 0; i < CinemachineBrain.ActiveBrainCount; ++i)
                {
                    var brain = CinemachineBrain.GetActiveBrain(i);
                    if (brain != null)
                        brain.DefaultBlend = new CinemachineBlendDefinition(blendType.Value, blendDuration);
                }
            }
            
            _maxPriority++;
            CurrentCamera.Priority.Value = _maxPriority;
            Debug.Log($"[CameraManager] {camera.name} Priority set to: {CurrentCamera.Priority.Value}");
            CurrentCamera.Prioritize();

            OnActiveCameraChanged.Invoke(CurrentCamera);
        }

        /// <summary>
        /// Switches the current camera back to the default one.
        /// </summary>
        public void ResetActiveCamera(CinemachineBlendDefinition.Styles? blendType = null, float blendDuration = 0)
        {
            Debug.Log("[CameraManager] Resetting to Default Camera.");
            if (SceneDefaultCamera == null)
            {
                Debug.LogWarning("[CameraManager] Cannot reset: SceneDefaultCamera is null.");
                return;
            }

            ChangeActiveCamera(SceneDefaultCamera, blendType, blendDuration);
        }

        private void Update()
        {
            CameraShaker.Update();
        }
        
        private void SceneManager_ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            Debug.Log($"[CameraManager] Scene changed from {oldScene.name} to {newScene.name}. Clearing camera references.");
            SceneDefaultCamera = null;
            CurrentCamera = null;
            _maxPriority = 10;
        }
    }
}
