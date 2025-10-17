using MoreMountains.Tools;
using UnityEngine;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Add this to a light 2D to have it receive MMLightShakeEvents from feedbacks or to shake it locally
	/// </summary>
	#if MM_URP
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Lights/MM Light 2DShaker URP")]
	[RequireComponent(typeof(Light2D))]
	#endif
	public class MMLight2DShaker_URP : MMShaker
	{
		#if MM_URP
		[MMInspectorGroup("Light", true, 37)]
		/// the light to affect when playing the feedback
		[Tooltip("the light to affect when playing the feedback")]
		public Light2D BoundLight;
		/// whether or not that light should be turned off on start
		[Tooltip("whether or not that light should be turned off on start")]
		public bool StartsOff = true;
		/// whether or not the values should be relative or not
		[Tooltip("whether or not the values should be relative or not")]
		public bool RelativeValues = true;

		[MMInspectorGroup("Color", true, 41)]
		/// whether or not this shaker should modify color 
		[Tooltip("whether or not this shaker should modify color")]
		public bool ModifyColor = true;
		/// the colors to apply to the light over time
		[Tooltip("the colors to apply to the light over time")]
		public Gradient ColorOverTime;

		[MMInspectorGroup("Intensity", true, 40)]
		/// the intensity to apply to the light over time
		/// the curve to tween the intensity on
		[Tooltip("the intensity to apply to the light over time")]
		public AnimationCurve IntensityCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the intensity curve's 0 to
		[Tooltip("the value to remap the intensity curve's 0 to")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the intensity curve's 1 to
		[Tooltip("the value to remap the intensity curve's 1 to")]
		public float RemapIntensityOne = 1f;

		[MMInspectorGroup("Range", true, 39)]
		/// the range to apply to the light over time
		[Tooltip("the range to apply to the light over time")]
		public AnimationCurve FalloffCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the range curve's 0 to
		[Tooltip("the value to remap the range curve's 0 to")]
		public float FalloffRangeZero = 0f;
		/// the value to remap the range curve's 0 to
		[Tooltip("the value to remap the range curve's 0 to")]
		public float RemapFalloffOne = 10f;

		[MMInspectorGroup("Shadow Strength", true, 38)]
		/// the range to apply to the light over time
		[Tooltip("the range to apply to the light over time")]
		public AnimationCurve ShadowStrengthCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the shadow strength's curve's 0 to
		[Tooltip("the value to remap the shadow strength's curve's 0 to")]
		public float RemapShadowStrengthZero = 0f;
		/// the value to remap the shadow strength's curve's 1 to
		[Tooltip("the value to remap the shadow strength's curve's 1 to")]
		public float RemapShadowStrengthOne = 1f;

		protected Color _initialColor;
		protected float _initialRange;
		protected float _initialIntensity;
		protected float _initialShadowStrength;

		protected bool _originalRelativeValues;
		protected bool _originalModifyColor;
		protected float _originalShakeDuration;
		protected Gradient _originalColorOverTime;
		protected AnimationCurve _originalIntensityCurve;
		protected float _originalRemapIntensityZero;
		protected float _originalRemapIntensityOne;
		protected AnimationCurve _originalRangeCurve;
		protected float _originalRemapRangeZero;
		protected float _originalRemapRangeOne;
		protected AnimationCurve _originalShadowStrengthCurve;
		protected float _originalRemapShadowStrengthZero;
		protected float _originalRemapShadowStrengthOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (BoundLight == null)
			{
				BoundLight = this.gameObject.GetComponent<Light2D>();
			}
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 1f;
		}
               
		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newRange = ShakeFloat(FalloffCurve, FalloffRangeZero, RemapFalloffOne, RelativeValues, _initialRange);
			BoundLight.shapeLightFalloffSize = newRange;
			float newIntensity = ShakeFloat(IntensityCurve, RemapIntensityZero, RemapIntensityOne, RelativeValues, _initialIntensity);
			BoundLight.intensity = newIntensity;
			float newShadowStrength = ShakeFloat(ShadowStrengthCurve, RemapShadowStrengthZero, RemapShadowStrengthOne, RelativeValues, _initialShadowStrength);
			BoundLight.shadowIntensity = Mathf.Clamp01(newShadowStrength);
			if (ModifyColor)
			{
				BoundLight.color = ColorOverTime.Evaluate(_remappedTimeSinceStart);
			}            
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialColor = BoundLight.color;
			_initialRange = BoundLight.shapeLightFalloffSize;
			_initialIntensity = BoundLight.intensity;
			_initialShadowStrength = BoundLight.shadowIntensity;
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			BoundLight.color = _initialColor;
			BoundLight.shapeLightFalloffSize = _initialRange;
			BoundLight.intensity = _initialIntensity;
			BoundLight.shadowIntensity = _initialShadowStrength;
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ModifyColor = _originalModifyColor;
			RelativeValues = _originalRelativeValues;
			ShakeDuration = _originalShakeDuration;
			ColorOverTime = _originalColorOverTime;
			IntensityCurve = _originalIntensityCurve;
			RemapIntensityZero = _originalRemapIntensityZero;
			RemapIntensityOne = _originalRemapIntensityOne;
			FalloffCurve = _originalRangeCurve;
			FalloffRangeZero  =_originalRemapRangeZero;
			RemapFalloffOne = _originalRemapRangeOne;
			ShadowStrengthCurve = _originalShadowStrengthCurve;
			RemapShadowStrengthZero = _originalRemapShadowStrengthZero;
			RemapShadowStrengthOne = _originalRemapShadowStrengthOne;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMLight2DShakeEvent.Register(OnMMLight2DShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMLight2DShakeEvent.Unregister(OnMMLight2DShakeEvent);
		}

		public virtual void OnMMLight2DShakeEvent(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!CheckEventAllowed(channelData, useRange, eventRange, eventOriginPosition) || (!Interruptible && Shaking))
			{
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalModifyColor = ModifyColor;
				_originalRelativeValues = RelativeValues;
				_originalShakeDuration = ShakeDuration;
				_originalColorOverTime = ColorOverTime;
				_originalIntensityCurve = IntensityCurve;
				_originalRemapIntensityZero = RemapIntensityZero;
				_originalRemapIntensityOne = RemapIntensityOne;
				_originalRangeCurve = FalloffCurve;
				_originalRemapRangeZero = FalloffRangeZero;
				_originalRemapRangeOne = RemapFalloffOne;
				_originalShadowStrengthCurve = ShadowStrengthCurve;
				_originalRemapShadowStrengthZero = RemapShadowStrengthZero;
				_originalRemapShadowStrengthOne = RemapShadowStrengthOne;
			}

			if (!OnlyUseShakerValues)
			{
				ModifyColor = modifyColor;
				RelativeValues = relativeValues;
				ShakeDuration = shakeDuration;
				ColorOverTime = colorOverTime;
				IntensityCurve = intensityCurve;
				RemapIntensityZero = remapIntensityZero;
				RemapIntensityOne = remapIntensityOne;
				FalloffCurve = rangeCurve;
				FalloffRangeZero = remapRangeZero;
				RemapFalloffOne = remapRangeOne;
				ShadowStrengthCurve = shadowStrengthCurve;
				RemapShadowStrengthZero = remapShadowStrengthZero;
				RemapShadowStrengthOne = remapShadowStrengthOne;
			}

			Play();
		}
	}
           
	public struct MMLight2DShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime, 
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne, 
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3));

		static public void Trigger(float shakeDuration, bool relativeValues, bool modifyColor, Gradient colorOverTime,
			AnimationCurve intensityCurve, float remapIntensityZero, float remapIntensityOne,
			AnimationCurve rangeCurve, float remapRangeZero, float remapRangeOne,
			AnimationCurve shadowStrengthCurve, float remapShadowStrengthZero, float remapShadowStrengthOne,
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true,
			bool useRange = false, float eventRange = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			OnEvent?.Invoke(shakeDuration, relativeValues, modifyColor, colorOverTime,
				intensityCurve, remapIntensityZero, remapIntensityOne,
				rangeCurve, remapRangeZero, remapRangeOne,
				shadowStrengthCurve, remapShadowStrengthZero, remapShadowStrengthOne, 
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, 
				useRange, eventRange, eventOriginPosition);
		}
		#endif
	}
}