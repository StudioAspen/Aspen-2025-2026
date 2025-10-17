using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A base feedback to set a float on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_UIToolkitFloatBase : MMF_UIToolkit
	{
		/// a static bool used to disable all feedbacks of this type at once
		public enum Modes { Instant, Interpolate, ToDestination }

		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasCustomInspectors => true;
		
		[MMFInspectorGroup("Value", true, 16)]
		/// the selected color mode :
		/// None : nothing will happen,
		/// gradient : evaluates the color over time on that gradient, from left to right,
		/// interpolate : lerps from the current color to the destination one 
		[Tooltip("the selected mode :" +
		         "Instant : the value will change instantly to the target one," +
		         "Curve : the value will be interpolated along the curve," +
		         "interpolate : lerps from the current value to the destination one ")]
		public Modes Mode = Modes.Interpolate;
		/// whether or not the value should be applied relatively to the initial value
		[Tooltip("whether or not the value should be applied relatively to the initial value")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate, (int)Modes.Instant)]
		public bool RelativeValue = false;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// how long the color of the text should change over time
		[Tooltip("how long the color of the text should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate, (int)Modes.ToDestination)]
		public float Duration = 0.2f;
		/// the value to apply when in instant mode
		[Tooltip("the value to apply when in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantValue = 1f;

		/// the curve to use when interpolating towards the destination value
		[Tooltip("the curve to use when interpolating towards the destination value")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic, "", "Mode", (int)Modes.Interpolate, (int)Modes.ToDestination);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("the value to which the curve's 0 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapZero = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("the value to which the curve's 1 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapOne = 1f;
		/// the value to aim towards when in ToDestination mode
		[Tooltip("the value to aim towards when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationValue = 1f;

		protected float _initialValue;
		protected Coroutine _coroutine;

		/// <summary>
		/// On init we store our initial value
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
			_initialValue = GetInitialValue();
		}

		/// <summary>
		/// On Play we change our text's alpha
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}

			if (RelativeValue)
			{
				_initialValue = GetInitialValue();
			}

			switch (Mode)
			{
				case Modes.Instant:
					float newInstantValue = RelativeValue ? InstantValue + _initialValue : InstantValue;
					if (!NormalPlayDirection)
					{
						newInstantValue = _initialValue;
					}
					SetValue(newInstantValue);
					break;
				case Modes.Interpolate:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ChangeValue());
					break;
				case Modes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					_initialValue = GetInitialValue();
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ChangeValue());
					break;
			}
		}

		/// <summary>
		/// Changes the color of the text over time
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ChangeValue()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);
				ApplyTime(remappedTime);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			ApplyTime(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield break;
		}

		/// <summary>
		/// Stops the animation if needed
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
			IsPlaying = false;
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

		/// <summary>
		/// Applies the alpha change
		/// </summary>
		/// <param name="time"></param>
		protected virtual void ApplyTime(float time)
		{
			float newValue = 0f;
			if (Mode == Modes.Interpolate)
			{
				float startValue = RelativeValue ? CurveRemapZero + _initialValue : CurveRemapZero;
				float endValue = RelativeValue ? CurveRemapOne + _initialValue : CurveRemapOne;
				
				newValue = MMTween.Tween(time, 0f, 1f, startValue, endValue, Curve);    
			}
			else if (Mode == Modes.ToDestination)
			{
				newValue = MMTween.Tween(time, 0f, 1f, _initialValue, DestinationValue, Curve);
			}

			SetValue(newValue);
		}

		protected virtual void SetValue(float newValue)
		{
			
		}

		protected virtual float GetInitialValue()
		{
			return 0f;
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			SetValue(_initialValue);
		}
		
		/// <summary>
		/// On Validate, we init our curves conditions if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			if (string.IsNullOrEmpty(Curve.EnumConditionPropertyName))
			{
				Curve.EnumConditionPropertyName = "Mode";
				Curve.EnumConditions = new bool[32];
				Curve.EnumConditions[(int)Modes.Interpolate] = true;
				Curve.EnumConditions[(int)Modes.ToDestination] = true;
			}
		}
	}
}