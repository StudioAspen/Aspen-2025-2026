using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
using UnityEngine.Rendering;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to any object and it'll set the target frame rate and vsync count. Note that vsync count must be 0 for the target FPS to work.
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Performance/MM FPS Unlock")]
	public class MMFPSUnlock : MonoBehaviour
	{
		/// the target FPS you want the game to run at, that's up to how many times Update will run every second
		[Tooltip("the target FPS you want the game to run at, that's up to how many times Update will run every second")]
		public int TargetFPS = 300;

		/// the number of frames to wait before rendering the next one. 0 will render every frame, 1 will render every 2 frames, 5 will render every 5 frames, etc
		[Tooltip("the number of frames to wait before rendering the next one. 0 will render every frame, 1 will render every 2 frames, 5 will render every 5 frames, etc")]
		public int RenderFrameInterval = 0;

		[Range(0, 2)]
		/// whether vsync should be enabled or not (on a 60Hz screen, 1 : 60fps, 2 : 30fps, 0 : don't wait for vsync)
		[Tooltip("whether vsync should be enabled or not (on a 60Hz screen, 1 : 60fps, 2 : 30fps, 0 : don't wait for vsync)")]
		public int VSyncCount = 0;

		/// if this is true, the user can press a number key to change the target FPS (1 : 10fps, 2 : 20fps, etc)
		[Tooltip("if this is true, the user can press a number key to change the target FPS (1 : 10fps, 2 : 20fps, etc)")]
		public bool EnableNumberShortcuts = false;

		/// <summary>
		/// On start we change our target fps and vsync settings
		/// </summary>
		protected virtual void Start()
		{
			UpdateSettings();
		}

		/// <summary>
		/// On update we check for input if needed
		/// </summary>
		protected virtual void Update()
		{
			HandleInput();
		}

		/// <summary>
		/// When a value gets changed in the editor, we update our settings
		/// </summary>
		protected virtual void OnValidate()
		{
			UpdateSettings();
		}

		/// <summary>
		/// Updates the target frame rate value and vsync count setting
		/// </summary>
		protected virtual void UpdateSettings()
		{
			QualitySettings.vSyncCount = VSyncCount;
			Application.targetFrameRate = TargetFPS;
			OnDemandRendering.renderFrameInterval = RenderFrameInterval;
		}

		/// <summary>
		/// Checks for presses on 0-9 keys and changes the target FPS accordingly
		/// </summary>
		protected virtual void HandleInput()
		{
			if (!EnableNumberShortcuts)
			{
				return;
			}

			bool input;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit0].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad0);
			#endif
			if (input) { TargetFPS = 300; UpdateSettings(); }

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit1].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad1);
			#endif
			if (input) { TargetFPS = 10; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit2].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad2);
			#endif
			if (input) { TargetFPS = 20; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit3].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad3);
			#endif
			if (input) { TargetFPS = 30; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit4].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad4);
			#endif
			if (input) { TargetFPS = 40; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit5].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad5);
			#endif
			if (input) { TargetFPS = 50; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit6].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad6);
			#endif
			if (input) { TargetFPS = 60; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit7].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad7);
			#endif
			if (input) { TargetFPS = 70; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit8].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad8);
			#endif
			if (input) { TargetFPS = 80; UpdateSettings(); }
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			input = Keyboard.current[Key.Digit9].wasPressedThisFrame;
			#else
			input = Input.GetKeyDown(KeyCode.Keypad9);
			#endif
			if (input) { TargetFPS = 90; UpdateSettings(); }
		}
	}
}