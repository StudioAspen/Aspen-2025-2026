using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to pilot Vector2 springs
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("A feedback used to pilot Vector2 springs")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Springs/Spring Vector2")]
	public class MMF_SpringVector2 : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SpringColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true; 
		#endif

		/// the duration of this feedback is the duration of the zoom
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasChannel => true;
		public override bool CanForceInitialValue => true;

		[MMFInspectorGroup("Spring", true, 72)] 
		
		/// the Vector2 spring we want to pilot using this feedback. If you set one, only that spring will be targeted. If you don't, an event will be sent out to all springs matching the channel data info
		[Tooltip("the Vector2 spring we want to pilot using this feedback. If you set one, only that spring will be targeted. If you don't, an event will be sent out to all springs matching the channel data info")]
		public MMSpringComponentBase TargetSpring;
		
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;
		
		/// the command to use on that spring
		[Tooltip("the command to use on that spring")]
		public SpringCommands Command = SpringCommands.Bump;
		[MMEnumCondition("Command", (int)SpringCommands.MoveTo, (int)SpringCommands.MoveToAdditive, (int)SpringCommands.MoveToSubtractive, (int)SpringCommands.MoveToInstant)]
		/// the new value this spring should move towards
		[Tooltip("the new value this spring should move towards")]
		public Vector2 MoveToValue = new Vector2(2f, 2f);
		/// the amount to add to the spring's current velocity to disturb it and make it bump
		[Tooltip("the amount to add to the spring's current velocity to disturb it and make it bump")]
		[MMEnumCondition("Command", (int)SpringCommands.Bump)]
		public Vector2 BumpAmount = new Vector2(75f, 75f);
		
		/// the min values between which a random target x value will be picked when calling MoveToRandom
		[Tooltip("the min values between which a random target x value will be picked when calling MoveToRandom")]
		[MMEnumCondition("Command", (int)SpringCommands.MoveToRandom)]
		public Vector2 MoveToRandomValueMin = new Vector2(-2f, -2f);
		/// the min (x) and max (y) values between which a random target y value will be picked when calling MoveToRandom
		[Tooltip("the min (x) and max (y) values between which a random target y value will be picked when calling MoveToRandom")]
		[MMEnumCondition("Command", (int)SpringCommands.MoveToRandom)]
		public Vector2 MoveToRandomValueMax = new Vector2(2f, 2f);
		
		/// the min (x) and max (y) values between which a random bump x value will be picked when calling BumpRandom
		[Tooltip("the min (x) and max (y) values between which a random bump x value will be picked when calling BumpRandom")]
		[MMEnumCondition("Command", (int)SpringCommands.BumpRandom)]
		public Vector2 BumpAmountRandomValueMin = new Vector2(-20f, -20f);
		/// the min (x) and max (y) values between which a random bump y value will be picked when calling BumpRandom
		[Tooltip("the min (x) and max (y) values between which a random bump y value will be picked when calling BumpRandom")]
		[MMEnumCondition("Command", (int)SpringCommands.BumpRandom)]
		public Vector2 BumpAmountRandomValueMax = new Vector2(20f, 20f);
		
		[Header("Overrides")]
		/// whether or not to override the current Damping value of the target spring(s) with the one specified below (NewDamping)
		[Tooltip("whether or not to override the current Damping value of the target spring(s) with the one specified below (NewDamping)")]
		public bool OverrideDamping = false;
		/// the new damping value to apply to the target spring(s) if OverrideDamping is true
		[Tooltip("the new damping value to apply to the target spring(s) if OverrideDamping is true")]
		[MMFCondition("OverrideDamping", true)]
		public Vector2 NewDamping = new Vector2(0.8f, 0.8f);
		/// whether or not to override the current Frequency value of the target spring(s) with the one specified below (NewFrequency)
		[Tooltip("whether or not to override the current Frequency value of the target spring(s) with the one specified below (NewFrequency)")]
		public bool OverrideFrequency = false;
		/// the new frequency value to apply to the target spring(s) if OverrideFrequency is true
		[Tooltip("the new frequency value to apply to the target spring(s) if OverrideFrequency is true")]
		[MMFCondition("OverrideFrequency", true)]
		public Vector2 NewFrequency = new Vector2(5f, 5f);

		protected MMChannelData _eventChannelData;

		/// <summary>
		/// On Play, triggers a spring event with the selected settings
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(Command, TargetSpring, _eventChannelData, MoveToValue, BumpAmount,
				MoveToRandomValueMin, MoveToRandomValueMax,
				BumpAmountRandomValueMin, BumpAmountRandomValueMax,
				OverrideDamping, NewDamping, OverrideFrequency, NewFrequency);
		}

		/// <summary>
		/// On stop, triggers a spring stop event
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
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(SpringCommands.Stop, TargetSpring, _eventChannelData);
		}
		
		/// <summary>
		/// On restore, triggers a spring RestoreInitialValue event
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_eventChannelData = (TargetSpring == null) ? ChannelData : null;
			MMSpringVector2Event.Trigger(SpringCommands.RestoreInitialValue, TargetSpring, _eventChannelData);
		}
	}
}