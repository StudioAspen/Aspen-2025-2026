using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A base feedback to set a vector2 on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_UIToolkitVector2Base : MMF_UIToolkit
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
		public bool RelativeValues = false;
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
		public Vector2 InstantValue = new Vector2(1f, 1f);

		[Header("X")]
		/// whether or not to animate the x value
		[Tooltip("whether or not to animate the x value")]
		public bool AnimateX = true;
		/// the curve to use when interpolating towards the destination value
		[Tooltip("the curve to use when interpolating towards the destination value")]
		public MMTweenType CurveX = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic, "", "Mode", (int)Modes.Interpolate, (int)Modes.ToDestination);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("the value to which the curve's 0 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapZeroX = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("the value to which the curve's 1 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapOneX = 1f;
		/// the value to aim towards when in ToDestination mode
		[Tooltip("the value to aim towards when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationValueX = 1f;
		
		[Header("Y")]
		/// whether or not to animate the y value
		[Tooltip("whether or not to animate the y value")]
		public bool AnimateY = true;
		/// the curve to use when interpolating towards the destination value
		[Tooltip("the curve to use when interpolating towards the destination value")]
		public MMTweenType CurveY = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic, "", "Mode", (int)Modes.Interpolate, (int)Modes.ToDestination);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("the value to which the curve's 0 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapZeroY = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("the value to which the curve's 1 should be remapped")]
		[MMFEnumCondition("Mode", (int)Modes.Interpolate)]
		public float CurveRemapOneY = 1f;
		/// the value to aim towards when in ToDestination mode
		[Tooltip("the value to aim towards when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationValueY = 1f;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;

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

			if (RelativeValues)
			{
				_initialValue = GetInitialValue();
			}

			switch (Mode)
			{
				case Modes.Instant:
					Vector2 newInstantValue = RelativeValues ? InstantValue + _initialValue : InstantValue;
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
			_newValue.x = _initialValue.x;
			_newValue.y = _initialValue.y;
			
			if (Mode == Modes.Interpolate)
			{
				if (AnimateX)
				{
					float startValueX = RelativeValues ? CurveRemapZeroX + _initialValue.x : CurveRemapZeroX;
					float endValueX = RelativeValues ? CurveRemapOneX + _initialValue.x : CurveRemapOneX;
					_newValue.x = MMTween.Tween(time, 0f, 1f, startValueX, endValueX, CurveX);	
				}

				if (AnimateY)
				{
					float startValueY = RelativeValues ? CurveRemapZeroY + _initialValue.y : CurveRemapZeroY;
					float endValueY = RelativeValues ? CurveRemapOneY + _initialValue.y : CurveRemapOneY;
					_newValue.y = MMTween.Tween(time, 0f, 1f, startValueY, endValueY, CurveY);	
				}
			}
			else if (Mode == Modes.ToDestination)
			{
				if (AnimateX)
				{
					_newValue.x = MMTween.Tween(time, 0f, 1f, _initialValue.x, DestinationValueX, CurveX);	
				}
				if (AnimateY)
				{
					_newValue.y = MMTween.Tween(time, 0f, 1f, _initialValue.y, DestinationValueY, CurveY);	
				}
			}

			SetValue(_newValue);
		}

		protected virtual void SetValue(Vector2 newValue)
		{
			
		}

		protected virtual Vector2 GetInitialValue()
		{
			return Vector2.zero;
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
			if (string.IsNullOrEmpty(CurveX.EnumConditionPropertyName))
			{
				CurveX.EnumConditionPropertyName = "Mode";
				CurveX.EnumConditions = new bool[32];
				CurveX.EnumConditions[(int)Modes.Interpolate] = true;
				CurveX.EnumConditions[(int)Modes.ToDestination] = true;
				CurveY.EnumConditions = new bool[32];
				CurveY.EnumConditionPropertyName = "Mode";
				CurveY.EnumConditions[(int)Modes.Interpolate] = true;
				CurveY.EnumConditions[(int)Modes.ToDestination] = true;
			}
		}
	}
}