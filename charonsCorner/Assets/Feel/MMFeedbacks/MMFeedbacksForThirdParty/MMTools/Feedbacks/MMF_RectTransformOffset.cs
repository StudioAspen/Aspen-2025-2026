using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the offset of the lower left corner of the rectangle relative to the lower left anchor, and the offset of the upper right corner of the rectangle relative to the upper right anchor.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the offset of the lower left corner of the rectangle relative to the lower left anchor, and the offset of the upper right corner of the rectangle relative to the upper right anchor.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[System.Serializable]
	[FeedbackPath("UI/RectTransform Offset")]
	public class MMF_RectTransformOffset : MMF_FeedbackBase
	{
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRectTransform == null); }
		public override string RequiredTargetText { get { return TargetRectTransform != null ? TargetRectTransform.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRectTransform be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => TargetRectTransform = FindAutomatedTarget<RectTransform>();

		[MMFInspectorGroup("Target RectTransform", true, 37, true)]
		/// The RectTransform we want to modify
		public RectTransform TargetRectTransform;

		[MMFInspectorGroup("Offset Min", true, 40)] 
		/// whether we should modify the offset min or not
		[Tooltip("whether we should modify the offset min or not")]
		public bool ModifyOffsetMin = true;
		/// the curve to animate the min offset on
		[Tooltip("the curve to animate the min offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType OffsetMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min curve's 0 on
		[Tooltip("the value to remap the min curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMinRemapZero = Vector2.zero;
		/// the value to remap the min curve's 1 on
		[Tooltip("the value to remap the min curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMinRemapOne = Vector2.one;
		/// the offset min to reach when in ToDestination mode
		[Tooltip("the offset min to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector2 DestinationOffsetMin = Vector2.zero;
        
		[MMFInspectorGroup("Offset Max", true, 41)] 
		/// whether we should modify the offset max or not
		[Tooltip("whether we should modify the offset max or not")]
		public bool ModifyOffsetMax = true;
		/// the curve to animate the max offset on
		[Tooltip("the curve to animate the max offset on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType OffsetMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max curve's 0 on
		[Tooltip("the value to remap the max curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 OffsetMaxRemapZero = Vector2.zero;
		/// the value to remap the max curve's 1 on
		[Tooltip("the value to remap the max curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 OffsetMaxRemapOne = Vector2.one;
		/// the offset max to reach when in ToDestination mode
		[Tooltip("the offset max to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector2 DestinationOffsetMax = Vector2.zero;

		protected Vector2 _initialOffsetMin;
		protected Vector2 _initialOffsetMax;
		protected Vector2 _newMinValue;
		protected Vector2 _newMaxValue;

		/// <summary>
		/// On init we store our initial offsets
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetRectTransform != null))
			{
				_initialOffsetMin = TargetRectTransform.offsetMin;
				_initialOffsetMax = TargetRectTransform.offsetMax;
			}
		}

		/// <summary>
		/// On Play we handle ToDestination mode ourselves, and delegate other modes to the base class
		/// </summary>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetRectTransform == null))
			{
				return;
			}

			if (Mode == Modes.ToDestination)
			{
				if (!AllowAdditivePlays && (_coroutine != null))
				{
					return;
				}
				if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
				_initialOffsetMin = TargetRectTransform.offsetMin;
				_initialOffsetMax = TargetRectTransform.offsetMax;
				_coroutine = Owner.StartCoroutine(OffsetToDestinationCo(feedbacksIntensity, position));
			}
			else
			{
				base.CustomPlayFeedback(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Coroutine that lerps the offsets from their current values to the destination values
		/// </summary>
		protected virtual IEnumerator OffsetToDestinationCo(float feedbacksIntensity, Vector3 position)
		{
			if (FeedbackDuration == 0f)
			{
				if (ModifyOffsetMin)
				{
					TargetRectTransform.offsetMin = NormalPlayDirection ? DestinationOffsetMin : _initialOffsetMin;
				}
				if (ModifyOffsetMax)
				{
					TargetRectTransform.offsetMax = NormalPlayDirection ? DestinationOffsetMax : _initialOffsetMax;
				}
				_coroutine = null;
				IsPlaying = false;
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			_newMinValue = _initialOffsetMin;
			_newMaxValue = _initialOffsetMax;
			IsPlaying = true;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				if (ModifyOffsetMin)
				{
					_newMinValue.x = MMTween.Tween(percent, 0f, 1f, _initialOffsetMin.x, DestinationOffsetMin.x, OffsetMinCurve);
					_newMinValue.y = MMTween.Tween(percent, 0f, 1f, _initialOffsetMin.y, DestinationOffsetMin.y, OffsetMinCurve);
					TargetRectTransform.offsetMin = _newMinValue;
				}

				if (ModifyOffsetMax)
				{
					_newMaxValue.x = MMTween.Tween(percent, 0f, 1f, _initialOffsetMax.x, DestinationOffsetMax.x, OffsetMaxCurve);
					_newMaxValue.y = MMTween.Tween(percent, 0f, 1f, _initialOffsetMax.y, DestinationOffsetMax.y, OffsetMaxCurve);
					TargetRectTransform.offsetMax = _newMaxValue;
				}

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			if (ModifyOffsetMin)
			{
				TargetRectTransform.offsetMin = NormalPlayDirection ? DestinationOffsetMin : _initialOffsetMin;
			}
			if (ModifyOffsetMax)
			{
				TargetRectTransform.offsetMax = NormalPlayDirection ? DestinationOffsetMax : _initialOffsetMax;
			}
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// On restore, we put our object back at its initial offsets
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.ToDestination)
			{
				if (ModifyOffsetMin)
				{
					TargetRectTransform.offsetMin = _initialOffsetMin;
				}
				if (ModifyOffsetMax)
				{
					TargetRectTransform.offsetMax = _initialOffsetMax;
				}
			}
			else
			{
				base.CustomRestoreInitialValues();
			}
		}
        
		protected override void FillTargets()
		{
			if (TargetRectTransform == null)
			{
				return;
			}
            
			MMF_FeedbackBaseTarget targetMin = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiverMin = new MMPropertyReceiver();
			receiverMin.TargetObject = TargetRectTransform.gameObject;
			receiverMin.TargetComponent = TargetRectTransform;
			receiverMin.TargetPropertyName = "offsetMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = OffsetMinRemapZero;
			receiverMin.Vector2RemapOne = OffsetMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyOffsetMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = OffsetMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMF_FeedbackBaseTarget targetMax = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "offsetMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = OffsetMaxRemapZero;
			receiverMax.Vector2RemapOne = OffsetMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyOffsetMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = OffsetMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}

	}
}