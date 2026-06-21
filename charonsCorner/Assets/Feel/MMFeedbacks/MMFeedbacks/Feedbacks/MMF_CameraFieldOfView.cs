using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a camera's field of view over time. You'll need a MMCameraFieldOfViewShaker on your camera.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Camera/Field of View")]
	[FeedbackHelp(
		"This feedback lets you control a camera's field of view over time. You'll need a MMCameraFieldOfViewShaker on your camera.")]
	public class MMF_CameraFieldOfView : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor => MMFeedbacksInspectorColors.CameraColor; 
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true; 
		public override bool HasAutomaticShakerSetup => true;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration
		{
			get { return ApplyTimeMultiplier(Duration); }
			set { Duration = value; }
		}

		public override bool HasChannel => true;
		public override bool CanForceInitialValue => true;
		public override bool ForceInitialValueDelayed => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Field of View", true, 37)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 2f;

		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;

		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeFieldOfView = false;

		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeFieldOfView =
			new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")] [Range(0f, 179f)]
		public float RemapFieldOfViewZero = 60f;

		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")] [Range(0f, 179f)]
		public float RemapFieldOfViewOne = 120f;

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, RelativeFieldOfView,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake,
				NormalPlayDirection, ComputedTimescaleMode);
		}

		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			base.CustomStopFeedback(position, feedbacksIntensity);
			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, stop: true);
		}

		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			MMCameraFieldOfViewShakeEvent.Trigger(ShakeFieldOfView, FeedbackDuration, RemapFieldOfViewZero,
				RemapFieldOfViewOne, restore: true);
		}
		
		/// <summary>
		/// Automatically tries to add a MMCameraFieldOfViewShaker to the main camera, if none are present
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			bool virtualCameraFound = false;
			#endif
			
			#if MMCINEMACHINE 
				CinemachineVirtualCamera virtualCamera = (CinemachineVirtualCamera)Object.FindObjectOfType(typeof(CinemachineVirtualCamera));
				virtualCameraFound = (virtualCamera != null);
			#elif MMCINEMACHINE3
				CinemachineCamera virtualCamera = (CinemachineCamera)Object.FindObjectOfType(typeof(CinemachineCamera));
				virtualCameraFound = (virtualCamera != null);
			#endif
			
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (virtualCameraFound)
			{
				MMCinemachineHelpers.AutomaticCinemachineShakersSetup(Owner, "CinemachineImpulse");
				return;
			}
			#endif
			
			MMCameraFieldOfViewShaker fieldOfViewShaker = (MMCameraFieldOfViewShaker)Object.FindAnyObjectByType(typeof(MMCameraFieldOfViewShaker));
			if (fieldOfViewShaker != null)
			{
				return;
			}

			Camera.main.gameObject.MMGetOrAddComponent<MMCameraFieldOfViewShaker>(); 
			MMDebug.DebugLogInfo( "Added a MMCameraFieldOfViewShaker to the main camera. You're all set.");
		}
	}
}