using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a CM camera's dutch angle over time. You'll need a MMCinemachineDutchShaker on your camera.
	/// </summary>
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Camera/Cinemachine Dutch")]
	[FeedbackHelp(
		"This feedback lets you control a Cinemachine camera's dutch angle (it tilt over the z axis) over time. You'll need a MMCinemachineDutchShaker on your camera.")]
	public class MMF_CinemachineCameraDutch : MMF_Feedback
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

		[MMFInspectorGroup("Dutch Angle", true, 37)]
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
		public bool RelativeDutch = false;

		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeDutch =
			new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")] [Range(0f, 179f)]
		public float RemapDutchZero = 0f;

		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")] [Range(0f, 179f)]
		public float RemapDutchOne = 20f;

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
			MMCameraDutchShakeEvent.Trigger(ShakeDutch, FeedbackDuration, RemapDutchZero,
				RemapDutchOne, RelativeDutch,
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
			MMCameraDutchShakeEvent.Trigger(ShakeDutch, FeedbackDuration, RemapDutchZero,
				RemapDutchOne, stop: true);
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

			MMCameraDutchShakeEvent.Trigger(ShakeDutch, FeedbackDuration, RemapDutchZero,
				RemapDutchOne, restore: true);
		}
		
		/// <summary>
		/// Automatically tries to add a MMCinemachineDutchShaker to the CM camera, if none are present
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			bool virtualCameraFound = false;
			#endif
			
			#if MM_CINEMACHINE 
				CinemachineVirtualCamera virtualCamera = (CinemachineVirtualCamera)Object.FindObjectOfType(typeof(CinemachineVirtualCamera));
				virtualCameraFound = (virtualCamera != null);
			#elif MM_CINEMACHINE3
				CinemachineCamera virtualCamera = (CinemachineCamera)Object.FindAnyObjectByType(typeof(CinemachineCamera), FindObjectsInactive.Include);
				virtualCameraFound = (virtualCamera != null);
			#endif
			
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (virtualCameraFound)
			{
				MMCinemachineHelpers.AutomaticCinemachineShakersSetup(Owner, "Dutch");
				return;
			}
			#endif
		}
	}
	#endif
}