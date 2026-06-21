using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you update a TMP text value over time, with a long value going from A to B over time, on a curve
	/// </summary>
	[AddComponentMenu("")]
	[System.Serializable]
	[FeedbackHelp("This feedback will let you update a TMP text value over time, with a long value going from A to B over time, on a curve")]
	#if MM_UGUI2
	[FeedbackPath("TextMesh Pro/TMP Count To Long")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.TextMeshPro")]
	public class MMF_TMPCountToLong : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetTMPText be set to be able to work properly. You can set one below."; } }
		#endif
		#if UNITY_EDITOR && MM_UGUI2
		public override bool EvaluateRequiresSetup() { return (TargetTMPText == null); }
		public override string RequiredTargetText { get { return TargetTMPText != null ? TargetTMPText.name : "";  } }
		#endif

		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
        
		#if MM_UGUI2
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTMPText = FindAutomatedTarget<TMP_Text>();

		[MMFInspectorGroup("TextMeshPro Target Text", true, 12, true)]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("the target TMP_Text component we want to change the text on")]
		public TMP_Text TargetTMPText;
		#endif
        
		[MMFInspectorGroup("Count Settings", true, 13)]
		/// the value from which to count from
		[Tooltip("the value from which to count from")]
		public long CountFrom = 0;
		/// the value to count towards
		[Tooltip("the value to count towards")]
		public long CountTo = 100000001;
		/// the curve on which to animate the count
		[Tooltip("the curve on which to animate the count")]
		public MMTweenType CountingCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f)));
		/// the duration of the count, in seconds
		[Tooltip("the duration of the count, in seconds")]
		public float Duration = 5f;
		/// the format with which to display the count
		[Tooltip("the format with which to display the count")]
		public string Format = "00.00";
		/// the minimum frequency (in seconds) at which to refresh the text field
		[Tooltip("the minimum frequency (in seconds) at which to refresh the text field")]
		public float MinRefreshFrequency = 0f;

		protected string _newText;
		protected float _startTime;
		protected float _lastRefreshAt;
		protected string _initialText;
		protected Coroutine _coroutine;
        
		/// <summary>
		/// On play we change the text of our target TMPText over time
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_UGUI2
			if (TargetTMPText == null)
			{
				return;
			}

			_initialText = TargetTMPText.text;
			#endif
			_coroutine = Owner.StartCoroutine(CountCo());
		}

		/// <summary>
		/// A coroutine used to animate the text
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator CountCo()
		{
			IsPlaying = true;
			_lastRefreshAt = -float.MaxValue;
			long currentValue = CountFrom;
			_startTime = FeedbackTime;
	        
			while (FeedbackTime - _startTime <= Duration)
			{
				if (FeedbackTime - _lastRefreshAt >= MinRefreshFrequency)
				{
					currentValue = ProcessCount();
					UpdateText(currentValue);
					_lastRefreshAt = FeedbackTime;
				}
		        
				yield return null;
			}
			UpdateText(CountTo);
			IsPlaying = false;
		}

		/// <summary>
		/// Updates the text of the target TMPText component with the updated value
		/// </summary>
		/// <param name="currentValue"></param>
		protected virtual void UpdateText(long currentValue)
		{
			_newText = currentValue.ToString(Format);
			#if MM_UGUI2
			TargetTMPText.text = _newText;
			#endif
		}

		/// <summary>
		/// Computes the new value of the count for the current time
		/// </summary>
		/// <param name="currentValue"></param>
		/// <returns></returns>
		protected virtual long ProcessCount()
		{
			float currentTime = FeedbackTime - _startTime;
			return MMTween.Tween(currentTime, 0f, Duration, CountFrom, CountTo, CountingCurve);
		}
		
		/// <summary>
		/// On stop, we interrupt counting if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
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
			#if MM_UGUI2
			TargetTMPText.text = _initialText;
			#endif
		}
	}
}