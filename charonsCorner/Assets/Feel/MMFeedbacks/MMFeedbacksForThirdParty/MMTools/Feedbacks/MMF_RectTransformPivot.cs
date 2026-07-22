using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the position of a RectTransform's pivot over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you control the position of a RectTransform's pivot over time")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[System.Serializable]
	[FeedbackPath("UI/RectTransform Pivot")]
	public class MMF_RectTransformPivot : MMF_FeedbackBase
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
		/// the RectTransform whose position you want to control over time 
		[Tooltip("the RectTransform whose position you want to control over time")]
		public RectTransform TargetRectTransform;
        
		[MMFInspectorGroup("Pivot", true, 39)] 
		/// The curve along which to evaluate the position of the RectTransform's pivot
		[Tooltip("The curve along which to evaluate the position of the RectTransform's pivot")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType SpeedCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the position to remap the curve's 0 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("the position to remap the curve's 0 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapZero = Vector2.zero;
		/// the position to remap the curve's 1 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("the position to remap the curve's 1 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime, (int)MMFeedbackBase.Modes.Instant)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapOne = Vector2.one;
		/// the pivot to reach when in ToDestination mode
		[Tooltip("the pivot to reach when in ToDestination mode")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		[MMFVector("Min", "Max")]
		public Vector2 DestinationPivot = new Vector2(0.5f, 0.5f);

		protected Vector2 _initialPivot;
		protected Vector2 _newValue;

		/// <summary>
		/// On init we store our initial pivot
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetRectTransform != null))
			{
				_initialPivot = TargetRectTransform.pivot;
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
				_initialPivot = TargetRectTransform.pivot;
				_coroutine = Owner.StartCoroutine(PivotToDestinationCo(feedbacksIntensity, position));
			}
			else
			{
				base.CustomPlayFeedback(position, feedbacksIntensity);
			}
		}

		/// <summary>
		/// Coroutine that lerps the pivot from its current value to the destination value
		/// </summary>
		protected virtual IEnumerator PivotToDestinationCo(float feedbacksIntensity, Vector3 position)
		{
			if (FeedbackDuration == 0f)
			{
				TargetRectTransform.pivot = NormalPlayDirection ? DestinationPivot : _initialPivot;
				_coroutine = null;
				IsPlaying = false;
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			_newValue = _initialPivot;
			IsPlaying = true;

			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				_newValue.x = MMTween.Tween(percent, 0f, 1f, _initialPivot.x, DestinationPivot.x, SpeedCurve);
				_newValue.y = MMTween.Tween(percent, 0f, 1f, _initialPivot.y, DestinationPivot.y, SpeedCurve);

				TargetRectTransform.pivot = _newValue;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			TargetRectTransform.pivot = NormalPlayDirection ? DestinationPivot : _initialPivot;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// On restore, we put our object back at its initial pivot
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.ToDestination)
			{
				TargetRectTransform.pivot = _initialPivot;
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
            
			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetRectTransform.gameObject;
			receiver.TargetComponent = TargetRectTransform;
			receiver.TargetPropertyName = "pivot";
			receiver.RelativeValue = RelativeValues;
			receiver.Vector2RemapZero = RemapZero;
			receiver.Vector2RemapOne = RemapOne;
			target.Target = receiver;
			target.LevelCurve = SpeedCurve;
			target.RemapLevelZero = 0f;
			target.RemapLevelOne = 1f;
			target.InstantLevel = 1f;

			_targets.Add(target);
		}
	}
}