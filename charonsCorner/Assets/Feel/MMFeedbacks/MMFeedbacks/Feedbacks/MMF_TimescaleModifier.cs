using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback changes the timescale by sending a TimeScale event on play
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback triggers a MMTimeScaleEvent, which, if you have a MMTimeManager object in your scene, will be caught and used to modify the timescale according to the specified settings. These settings are the new timescale (0.5 will be twice slower than normal, 2 twice faster, etc), the duration of the timescale modification, and the optional speed at which to transition between normal and altered time scale.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Time/Timescale Modifier")]
	public class MMF_TimescaleModifier : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// <summary>
		/// The possible modes for this feedback :
		/// - shake : changes the timescale for a certain duration
		/// - change : sets the timescale to a new value, forever (until you change it again)
		/// - reset : resets the timescale to its previous value
		/// </summary>
		public enum Modes { Shake, Change, Reset, Unfreeze }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		public override string RequiredTargetText { get { return Mode.ToString() + " x" + TimeScale ;  } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif

		[MMFInspectorGroup("Timescale Modifier", true, 63)]
		/// the selected mode
		[Tooltip("the selected mode : shake : changes the timescale for a certain duration" +
		         "\n- shake : sets the timescale to a new value for the specified TimeScaleDuration then reverts it back to what it was before" +
		         "\n- change : sets the timescale to a new value, forever (until you change it again)" +
		         "\n- reset : resets the timescale to its NormalTimescale value, defined in the MMTimeManager" +
		         "\n- unfreeze : sets the timescale back to its previous value, before the last change")]
		public Modes Mode = Modes.Shake;

		/// the new timescale to apply
		[Tooltip("the new timescale to apply")]
		public float TimeScale = 0.5f;
		/// the duration of the timescale modification
		[Tooltip("the duration of the timescale modification")]
		[MMFEnumCondition("Mode", (int)Modes.Shake)]
		public float TimeScaleDuration = 1f;
		/// whether to reset the timescale on Stop or not
		[Tooltip("whether to reset the timescale on Stop or not")]
		public bool ResetTimescaleOnStop = false;
		/// whether to unfreeze the timescale on Stop or not - if you set this to true, ResetTimescaleOnStop will be ignored
		[Tooltip("whether to unfreeze the timescale on Stop or not - if you set this to true, ResetTimescaleOnStop will be ignored")]
		public bool UnfreezeTimescaleOnStop = false;
		
		[MMFInspectorGroup("Interpolation", true, 63)]
		/// whether or not we should lerp the timescale
		[Tooltip("whether or not we should lerp the timescale")]
		public bool TimeScaleLerp = false;
		/// whether to lerp over a set duration, or at a certain speed
		[Tooltip("whether to lerp over a set duration, or at a certain speed")]
		public MMTimeScaleLerpModes TimescaleLerpMode = MMTimeScaleLerpModes.Speed;
		/// in Speed mode, the speed at which to lerp the timescale
		[Tooltip("in Speed mode, the speed at which to lerp the timescale")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Speed)]
		public float TimeScaleLerpSpeed = 1f;
		/// in Duration mode, the curve to use to lerp the timescale
		[Tooltip("in Duration mode, the curve to use to lerp the timescale")]
		public MMTweenType TimescaleLerpCurve = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)), 
			enumConditionPropertyName:"TimescaleLerpMode", enumConditionValues:(int)MMTimeScaleLerpModes.Duration); 
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds
		[Tooltip("in Duration mode, the duration of the timescale interpolation, in unscaled time seconds")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDuration = 1f;
		/// whether or not we should lerp the timescale as it goes back to normal afterwards when using Unfreeze mode
		[FormerlySerializedAs("TimeScaleLerpOnReset")]
		[Tooltip("whether or not we should lerp the timescale as it goes back to normal afterwards when using Unfreeze mode")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public bool TimeScaleLerpOnUnfreeze = false;
		/// in Duration mode, the curve to use to lerp the timescale when unfreezing if TimeScaleLerpOnUnfreeze is true
		[FormerlySerializedAs("TimescaleLerpCurveOnReset")] 
		[Tooltip("in Duration mode, the curve to use to lerp the timescale when unfreezing if TimeScaleLerpOnUnfreeze is true")]
		public MMTweenType TimescaleLerpCurveOnUnfreeze = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)), 
			enumConditionPropertyName:"TimescaleLerpMode", enumConditionValues:(int)MMTimeScaleLerpModes.Duration);
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds when unfreezing if TimeScaleLerpOnUnfreeze is true
		[FormerlySerializedAs("TimescaleLerpDurationOnReset")]
		[Tooltip("in Duration mode, the duration of the timescale interpolation, in unscaled time seconds when unfreezing if TimeScaleLerpOnUnfreeze is true")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDurationOnUnfreeze = 1f;

		/// the duration of this feedback is the duration of the time modification
		public override float FeedbackDuration {
			get
			{
				float totalDuration = (Mode == Modes.Shake) ? TimeScaleDuration : 0f;
				if (TimescaleLerpMode == MMTimeScaleLerpModes.Duration)
				{
					totalDuration += TimeScaleLerp ? TimescaleLerpDuration : 0f;
					if (Mode == Modes.Shake)
					{
						totalDuration += TimeScaleLerpOnUnfreeze ? TimescaleLerpDurationOnUnfreeze : 0f;
					}
				}
				return ApplyTimeMultiplier(totalDuration);
			}
			set
			{
				TimeScaleDuration = value;
			} }

		/// <summary>
		/// On Play, triggers a time scale event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.Shake:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, TimeScaleDuration, TimeScaleLerp, TimeScaleLerpSpeed, false, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnUnfreeze, TimescaleLerpCurveOnUnfreeze, TimescaleLerpDurationOnUnfreeze);
					break;
				case Modes.Change:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, 0f, TimeScaleLerp, TimeScaleLerpSpeed, true, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnUnfreeze, TimescaleLerpCurveOnUnfreeze, TimescaleLerpDurationOnUnfreeze);
					break;
				case Modes.Reset:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
					break;
				case Modes.Unfreeze:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, TimeScale, 0f, false, 0f, true);
					break;
			}     
		}

		/// <summary>
		/// On stop, we reset timescale if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (!ResetTimescaleOnStop && !UnfreezeTimescaleOnStop))
			{
				return;
			}
			if (UnfreezeTimescaleOnStop)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, TimeScale, 0f, false, 0f, true);
				return;
			}
			if (ResetTimescaleOnStop)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
				return;
			}
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
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
		}
		
		/// <summary>
		/// Automatically adds a MMTimeManager to the scene
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			(MMTimeManager timeManager, bool createdNew) = Owner.gameObject.MMFindOrCreateObjectOfType<MMTimeManager>("MMTimeManager", null);
			if (createdNew)
			{
				MMDebug.DebugLogInfo("Added a MMTimeManager to the scene. You're all set.");	
			}
		}
		
		/// <summary>
		/// On Validate, we init our curves conditions if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			if (string.IsNullOrEmpty(TimescaleLerpCurve.EnumConditionPropertyName))
			{
				TimescaleLerpCurve.EnumConditionPropertyName = "TimescaleLerpMode";
				TimescaleLerpCurveOnUnfreeze.EnumConditionPropertyName = "TimescaleLerpMode";
				TimescaleLerpCurve.EnumConditions = new bool[32];
			}
			if (TimescaleLerpCurve.EnumConditions[(int)MMTimeScaleLerpModes.Duration] == false)
			{
				TimescaleLerpCurve.EnumConditions[(int)MMTimeScaleLerpModes.Duration] = true;
				TimescaleLerpCurveOnUnfreeze.EnumConditions[(int)MMTimeScaleLerpModes.Duration] = true;
			}
		}
	}
}