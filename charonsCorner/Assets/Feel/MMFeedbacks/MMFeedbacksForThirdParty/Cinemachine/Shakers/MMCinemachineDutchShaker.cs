using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this to a Cinemachine camera and it'll let you control its dutch angle (it tilt over the z axis) over time, can be piloted by a MMF_CinemachineCameraDutch
	/// </summary>
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Dutch Shaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineDutchShaker : MMShaker
	{
		[MMInspectorGroup("Dutch Angle", true, 41)]
		/// whether or not to add to the initial value
		[Tooltip("whether or not to add to the initial value")]
		public bool RelativeDutch = false;
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the intensity value on")]
		public AnimationCurve ShakeDutch = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 179f)]
		public float RemapDutchZero = 60f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 179f)]
		public float RemapDutchOne = 120f;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _targetCamera;
		#elif  MM_CINEMACHINE3
		protected CinemachineCamera _targetCamera;
		#endif
		protected float _initialDutch;
		protected float _originalShakeDuration;
		protected bool _originalRelativeDutch;
		protected AnimationCurve _originalShakeDutch;
		protected float _originalRemapDutchZero;
		protected float _originalRemapDutchOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			#if MM_CINEMACHINE
			_targetCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
			#elif  MM_CINEMACHINE3
			_targetCamera = this.gameObject.GetComponent<CinemachineCamera>();
			#endif
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newDutch = ShakeFloat(ShakeDutch, RemapDutchZero, RemapDutchOne, RelativeDutch, _initialDutch);
			SetDutch(newDutch);
		}

		protected virtual void SetDutch(float newDutch)
		{
			#if MM_CINEMACHINE
			_targetCamera.m_Lens.Dutch = newDutch;
			#elif  MM_CINEMACHINE3
			_targetCamera.Lens.Dutch = newDutch;
			#endif
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			#if MM_CINEMACHINE
			_initialDutch = _targetCamera.m_Lens.Dutch;
			#elif  MM_CINEMACHINE3
			_initialDutch = _targetCamera.Lens.Dutch;
			#endif
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="distortionCurve"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeDistortion"></param>
		/// <param name="feedbacksIntensity"></param>
		/// <param name="channel"></param>
		public virtual void OnMMCameraDutchShakeEvent(AnimationCurve distortionCurve, float duration, float remapMin, float remapMax, bool relativeDistortion = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}

			if (restore)
			{
				ResetTargetValues();
				return;
			}
            
			if (!Interruptible && Shaking)
			{
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeDutch = ShakeDutch;
				_originalRemapDutchZero = RemapDutchZero;
				_originalRemapDutchOne = RemapDutchOne;
				_originalRelativeDutch = RelativeDutch;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeDutch = distortionCurve;
				RemapDutchZero = remapMin * feedbacksIntensity;
				RemapDutchOne = remapMax * feedbacksIntensity;
				RelativeDutch = relativeDistortion;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			SetDutch(_initialDutch);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeDutch = _originalShakeDutch;
			RemapDutchZero = _originalRemapDutchZero;
			RemapDutchOne = _originalRemapDutchOne;
			RelativeDutch = _originalRelativeDutch;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMCameraDutchShakeEvent.Register(OnMMCameraDutchShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMCameraDutchShakeEvent.Unregister(OnMMCameraDutchShakeEvent);
		}
	}
	
	public struct MMCameraDutchShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve animCurve, float duration, float remapMin, float remapMax, bool relativeValue = false,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, bool forwardDirection = true, 
			TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(animCurve, duration, remapMin, remapMax, relativeValue,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}