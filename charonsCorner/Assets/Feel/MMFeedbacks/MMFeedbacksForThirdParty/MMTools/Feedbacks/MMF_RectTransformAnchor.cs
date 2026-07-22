using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the min and max anchors of a RectTransform over time. That's the normalized position in the parent RectTransform that the lower left and upper right corners are anchored to.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the min and max anchors of a RectTransform over time. That's the normalized position in the parent RectTransform that the lower left and upper right corners are anchored to.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[System.Serializable]
	[FeedbackPath("UI/RectTransform Anchor")]
	public class MMF_RectTransformAnchor : MMF_FeedbackBase
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
		/// the target RectTransform to control
		[Tooltip("the target RectTransform to control")]
		public RectTransform TargetRectTransform;

		[MMFInspectorGroup("Anchor Min", true, 43)] 
		/// whether or not to modify the min anchor
		[Tooltip("whether or not to modify the min anchor")]
		public bool ModifyAnchorMin = true;
		/// the curve to animate the min anchor on
		[Tooltip("the curve to animate the min anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType AnchorMinCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the min anchor curve's 0 on
		[Tooltip("the value to remap the min anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMinRemapZero = Vector2.zero;
		/// the value to remap the min anchor curve's 1 on
		[Tooltip("the value to remap the min anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMinRemapOne = Vector2.one;
		/// the anchor min to reach when in ToDestination mode
		[Tooltip("the anchor min to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector2 DestinationAnchorMin = Vector2.zero;
        
		[MMFInspectorGroup("Anchor Max", true, 44)]
		/// whether or not to modify the max anchor
		[Tooltip("whether or not to modify the max anchor")]
		public bool ModifyAnchorMax = true;
		/// the curve to animate the max anchor on
		[Tooltip("the curve to animate the max anchor on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType AnchorMaxCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the value to remap the max anchor curve's 0 on
		[Tooltip("the value to remap the max anchor curve's 0 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public Vector2 AnchorMaxRemapZero = Vector2.zero;
		/// the value to remap the max anchor curve's 1 on
		[Tooltip("the value to remap the max anchor curve's 1 on")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		public Vector2 AnchorMaxRemapOne = Vector2.one;
		/// the anchor max to reach when in ToDestination mode
		[Tooltip("the anchor max to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector2 DestinationAnchorMax = Vector2.one;

		protected Vector2 _initialAnchorMin;
		protected Vector2 _initialAnchorMax;
		protected Vector2 _newMinValue;
		protected Vector2 _newMaxValue;

		/// <summary>
		/// On init we store our initial anchors
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetRectTransform != null))
			{
				_initialAnchorMin = TargetRectTransform.anchorMin;
				_initialAnchorMax = TargetRectTransform.anchorMax;
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
				_initialAnchorMin = TargetRectTransform.anchorMin;
				_initialAnchorMax = TargetRectTransform.anchorMax;
				_coroutine = Owner.StartCoroutine(AnchorToDestinationCo(feedbacksIntensity, position));
			}
			else
			{
				base.CustomPlayFeedback(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Coroutine that lerps the anchors from their current values to the destination values
		/// </summary>
		protected virtual IEnumerator AnchorToDestinationCo(float feedbacksIntensity, Vector3 position)
		{
			if (FeedbackDuration == 0f)
			{
				if (ModifyAnchorMin)
				{
					TargetRectTransform.anchorMin = NormalPlayDirection ? DestinationAnchorMin : _initialAnchorMin;
				}
				if (ModifyAnchorMax)
				{
					TargetRectTransform.anchorMax = NormalPlayDirection ? DestinationAnchorMax : _initialAnchorMax;
				}
				_coroutine = null;
				IsPlaying = false;
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			_newMinValue = _initialAnchorMin;
			_newMaxValue = _initialAnchorMax;
			IsPlaying = true;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				if (ModifyAnchorMin)
				{
					_newMinValue.x = MMTween.Tween(percent, 0f, 1f, _initialAnchorMin.x, DestinationAnchorMin.x, AnchorMinCurve);
					_newMinValue.y = MMTween.Tween(percent, 0f, 1f, _initialAnchorMin.y, DestinationAnchorMin.y, AnchorMinCurve);
					TargetRectTransform.anchorMin = _newMinValue;
				}

				if (ModifyAnchorMax)
				{
					_newMaxValue.x = MMTween.Tween(percent, 0f, 1f, _initialAnchorMax.x, DestinationAnchorMax.x, AnchorMaxCurve);
					_newMaxValue.y = MMTween.Tween(percent, 0f, 1f, _initialAnchorMax.y, DestinationAnchorMax.y, AnchorMaxCurve);
					TargetRectTransform.anchorMax = _newMaxValue;
				}

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			if (ModifyAnchorMin)
			{
				TargetRectTransform.anchorMin = NormalPlayDirection ? DestinationAnchorMin : _initialAnchorMin;
			}
			if (ModifyAnchorMax)
			{
				TargetRectTransform.anchorMax = NormalPlayDirection ? DestinationAnchorMax : _initialAnchorMax;
			}
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// On restore, we put our object back at its initial anchors
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.ToDestination)
			{
				if (ModifyAnchorMin)
				{
					TargetRectTransform.anchorMin = _initialAnchorMin;
				}
				if (ModifyAnchorMax)
				{
					TargetRectTransform.anchorMax = _initialAnchorMax;
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
			receiverMin.TargetPropertyName = "anchorMin";
			receiverMin.RelativeValue = RelativeValues;
			receiverMin.Vector2RemapZero = AnchorMinRemapZero;
			receiverMin.Vector2RemapOne = AnchorMinRemapOne;
			receiverMin.ShouldModifyValue = ModifyAnchorMin;
			targetMin.Target = receiverMin;
			targetMin.LevelCurve = AnchorMinCurve;
			targetMin.RemapLevelZero = 0f;
			targetMin.RemapLevelOne = 1f;
			targetMin.InstantLevel = 1f;

			_targets.Add(targetMin);
            
			MMF_FeedbackBaseTarget targetMax = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiverMax = new MMPropertyReceiver();
			receiverMax.TargetObject = TargetRectTransform.gameObject;
			receiverMax.TargetComponent = TargetRectTransform;
			receiverMax.TargetPropertyName = "anchorMax";
			receiverMax.RelativeValue = RelativeValues;
			receiverMax.Vector2RemapZero = AnchorMaxRemapZero;
			receiverMax.Vector2RemapOne = AnchorMaxRemapOne;
			receiverMax.ShouldModifyValue = ModifyAnchorMax;
			targetMax.Target = receiverMax;
			targetMax.LevelCurve = AnchorMaxCurve;
			targetMax.RemapLevelZero = 0f;
			targetMax.RemapLevelOne = 1f;
			targetMax.InstantLevel = 1f;

			_targets.Add(targetMax);
		}
	}
}