using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// Add this class to to a URP Volume to have it change weights on shake
	/// </summary>
	#if MM_URP
	[RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMVolumeBlendShaker_URP")]
	public class MMVolumeBlendShaker_URP : MMShaker
	{
		[MMInspectorGroup("Configuration", true, 50)]
		/// whether or not to add to the initial value
		public bool RelativeValues = true;
		[FormerlySerializedAs("disableVolumeOnWeightZero")] 
		public bool DisableVolumeOnWeightZero = true;
		[FormerlySerializedAs("disableVolumeOnStart")] 
		public bool DisableVolumeOnStart = true;
		
		[MMInspectorGroup("Blend Shake Intensity", true, 51)]
		/// the curve used to animate the intensity value on
		[Tooltip("the curve used to animate the amount of blend amount")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapIntensityOne = 1f;
		

		#if MM_URP
		protected Volume _volume;
		protected float _initialIntensity;
		protected float _originalShakeDuration;
		protected bool _originalRelativeIntensity;
		protected AnimationCurve _originalShakeIntensity;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		
		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			EvaluateEnableState();
			
			if (DisableVolumeOnStart)
			{
				_volume.enabled = false;
			}
			
			// _volume.profile.TryGet(out _bloom);
		}

		
		// /// <summary>
		// /// Shakes values over time
		// /// </summary>
		// [MMFInspectorButton("Shake")]
		protected override void Shake()
		{
			float newIntensity = ShakeFloat(ShakeIntensity, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
			_volume.weight = newIntensity;
			EvaluateEnableState();
			// _bloom.intensity.Override(newIntensity);
			// float newThreshold = ShakeFloat(ShakeThreshold, RemapThresholdZero, RemapThresholdOne, RelativeValues, _initialThreshold);
			// _bloom.threshold.Override(newThreshold);
		}

		private void EvaluateEnableState()
		{
			var currentWeight = _volume.weight;
			if (DisableVolumeOnWeightZero)
				_volume.enabled = currentWeight > 0;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialIntensity = _volume.weight;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnVolumeBlendShakeEvent(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
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
	            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalShakeIntensity = ShakeIntensity;
				_originalRemapIntensityZero = RemapIntensityZero;
				_originalRemapIntensityOne = RemapIntensityOne;
				_originalRelativeIntensity = RelativeValues;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeIntensity = intensity;
				RemapIntensityZero = remapMin * attenuation;
				RemapIntensityOne = remapMax * attenuation;
				RelativeValues = relativeIntensity;
				ForwardDirection = forwardDirection;
			}

			Play();
			
			EvaluateEnableState();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_volume.weight = _initialIntensity;
			// _bloom.threshold.Override(_initialThreshold);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			ShakeIntensity = _originalShakeIntensity;
			RemapIntensityZero = _originalRemapIntensityZero;
			RemapIntensityOne = _originalRemapIntensityOne;
			RelativeValues = _originalRelativeIntensity;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMVolumeBlendShakeEvent_URP.Register(OnVolumeBlendShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMVolumeBlendShakeEvent_URP.Unregister(OnVolumeBlendShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger blend volume shakes
	/// </summary>
	public struct MMVolumeBlendShakeEvent_URP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve intensity, float duration, float remapMin, float remapMax,
			bool relativeIntensity = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(intensity, duration, remapMin, remapMax, 
				relativeIntensity,
				attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}