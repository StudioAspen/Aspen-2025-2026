using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// The possible commands used to pilot a spring
	/// MoveTo : move the current value of the spring to the MoveToValue specified in the event
	/// MoveToAdditive : adds the MoveToValue specified in the event to the current target value of the spring
	/// MoveToSubtractive : subtracts the MoveToValue specified in the event to the current target value of the spring
	/// MoveToRandom : moves the current value of the spring to a random value using MoveToRandomValue
	/// MoveToInstant : instantly moves the current value of the spring to the MoveToValue specified in the event
	/// Bump : bumps the spring by the BumpAmount specified in the event
	/// BumpRandom : bumps the spring by a random amount specified in the event
	/// Stop : stops the spring instantly
	/// Finish : instantly moves the spring to its final target value
	/// RestoreInitialValue : restores the spring's initial value
	/// ResetInitialValue : resets the spring's initial value to its current value
	/// </summary>
	public enum SpringCommands { MoveTo, MoveToAdditive, MoveToSubtractive, MoveToRandom, MoveToInstant, Bump, BumpRandom, Stop, Finish, RestoreInitialValue, ResetInitialValue }
	
	/// <summary>
	/// An event used to pilot a MMSpringColor component
	/// </summary>
	public struct MMSpringFloatEvent
	{
		static MMSpringFloatEvent e;
		
		public MMChannelData ChannelData;
		public MMSpringComponentBase TargetSpring;
		public SpringCommands Command;
		public float MoveToValue;
		public float BumpAmount;
		public Vector2 MoveToRandomValue;
		public Vector2 BumpAmountRandomValue;
		public bool OverrideDamping;
		public float NewDamping;
		public bool OverrideFrequency;
		public float NewFrequency;
		
		public static void Trigger(SpringCommands command, MMSpringComponentBase targetSpring, MMChannelData channelData, 
			float moveToValue = 1f, float bumpAmount = 1f, Vector2 moveToRandomValue = default, Vector2 bumpAmountRandomValue = default, 
			bool overrideDamping = false, float newDamping = 0.8f, bool overrideFrequency = false, float newFrequency = 5f)
		{
			e.ChannelData = channelData;
			e.TargetSpring = targetSpring;
			e.Command = command;
			e.MoveToValue = moveToValue;
			e.BumpAmount = bumpAmount;
			e.MoveToRandomValue = moveToRandomValue;
			e.BumpAmountRandomValue = bumpAmountRandomValue;
			e.OverrideDamping = overrideDamping;
			e.NewDamping = newDamping;
			e.OverrideFrequency = overrideFrequency;
			e.NewFrequency = newFrequency;
			MMEventManager.TriggerEvent(e);
		}
	}	
	
	/// <summary>
	/// A spring component used to pilot float values on a target
	/// </summary>
	public abstract class MMSpringFloatComponent<T> : MMSpringComponentBase, MMEventListener<MMSpringFloatEvent> where T:Component
	{
		[MMInspectorGroup("Target", true, 17)] 
		public T Target;
		
		[MMInspectorGroup("Channel & TimeScale", true, 16, true)] 
		/// whether this spring should run on scaled time (and be impacted by time scale changes) or unscaled time (and not be impacted by time scale changes)
		[Tooltip("whether this spring should run on scaled time (and be impacted by time scale changes) or unscaled time (and not be impacted by time scale changes)")]
		public TimeScaleModes TimeScaleMode = TimeScaleModes.Scaled;
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what. " +
		         "MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("the channel to listen to - has to match the one on the feedback")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel, " +
		         "right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		
		[MMInspectorGroup("Spring Settings", true, 18)]
		public MMSpringFloat FloatSpring = new MMSpringFloat();
		
		[MMInspectorGroup("Randomness", true, 12, true)]
		/// the min (x) and max (y) values between which a random target value will be picked when calling MoveToRandom
		[Tooltip("the min (x) and max (y) values between which a random target value will be picked when calling MoveToRandom")]
		[MMVector("Min", "Max")]
		public Vector2 MoveToRandomValue = new Vector2(-2f, 2f);
		/// the min (x) and max (y) values between which a random bump value will be picked when calling BumpRandom
		[Tooltip("the min (x) and max (y) values between which a random bump value will be picked when calling BumpRandom")]
		[MMVector("Min", "Max")]
		public Vector2 BumpAmountRandomValue = new Vector2(20f, 100f);
		
		[MMInspectorGroup("Test", true, 20, true)]
		/// the value to move this spring to when interacting with any of the MoveTo debug buttons in its inspector
		[Tooltip("the value to move this spring to when interacting with any of the MoveTo debug buttons in its inspector")]
		public float TestMoveToValue = 2f;
		[MMInspectorButtonBar(new string[] { "MoveTo", "MoveToAdditive", "MoveToSubtractive", "MoveToRandom", "MoveToInstant" }, 
			new string[] { "TestMoveTo", "TestMoveToAdditive", "TestMoveToSubtractive", "TestMoveToRandom", "TestMoveToInstant" }, 
			new bool[] { true, true, true, true, true },
		new string[] { "main-call-to-action", "", "", "", "" })]
		public bool MoveToToolbar;
		
		/// the amount by which to bump this spring when interacting with the Bump debug button in its inspector
		[Tooltip("the amount by which to bump this spring when interacting with the Bump debug button in its inspector")]
		public float TestBumpAmount = 75f;
		[MMInspectorButtonBar(new string[] { "Bump", "BumpRandom" }, 
			new string[] { "TestBump", "TestBumpRandom" }, 
			new bool[] { true, true },
			new string[] { "main-call-to-action", "" })]
		public bool BumpToToolbar;
		
		[MMInspectorButtonBar(new string[] { "Stop", "Finish", "RestoreInitialValue", "ResetInitialValue" }, 
			new string[] { "Stop", "Finish", "RestoreInitialValue", "ResetInitialValue" }, 
			new bool[] { true, true, true, true },
			new string[] { "", "", "", "" })]
		public bool OtherControlsToToolbar;
		
		public override bool LowVelocity => Mathf.Abs(FloatSpring.Velocity) < _velocityLowThreshold;
		public float DeltaTime => (TimeScaleMode == TimeScaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;

		public virtual float TargetFloat { get; set; }
		
		#region PUBLIC_API
		
		public virtual void MoveTo(float newValue)
		{
			Activate();
			FloatSpring.MoveTo(newValue);
		}
		
		public virtual void MoveToAdditive(float newValue)
		{
			Activate();
			FloatSpring.MoveToAdditive(newValue);
		}
		
		public virtual void MoveToSubtractive(float newValue)
		{
			Activate();
			FloatSpring.MoveToSubtractive(newValue);
		}

		public virtual void MoveToRandom()
		{
			Activate();
			FloatSpring.MoveToRandom(MoveToRandomValue.x, MoveToRandomValue.y);
		}

		public virtual void MoveToInstant(float newValue)
		{
			Activate();
			FloatSpring.MoveToInstant(newValue);
		}

		public virtual void MoveToRandom(float min, float max)
		{
			Activate();
			FloatSpring.MoveToRandom(min, max);
		}

		public virtual void Bump(float bumpAmount)
		{
			Activate();
			FloatSpring.Bump(bumpAmount);
		}

		public virtual void BumpRandom()
		{
			Activate();
			FloatSpring.BumpRandom(BumpAmountRandomValue.x, BumpAmountRandomValue.y);
		}

		public virtual void BumpRandom(float min, float max)
		{
			Activate();
			FloatSpring.BumpRandom(min, max);
		}
		
		public override void Stop()
		{
			base.Stop();
			this.enabled = false;
			GrabCurrentValue();
			FloatSpring.Stop();
		}
		
		public override void RestoreInitialValue()
		{
			FloatSpring.RestoreInitialValue();
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		public override void ResetInitialValue()
		{
			FloatSpring.SetCurrentValueAsInitialValue();
		}
		
		protected override void UpdateSpringValue()
		{
			FloatSpring.UpdateSpringValue(DeltaTime);
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		public override void Finish()
		{
			FloatSpring.Finish();
			ApplyValue(FloatSpring.CurrentValue);
		}
		
		#endregion

		#region INTERNAL
		
		protected override void Initialization()
		{
			base.Initialization();
			GrabCurrentValue();
			FloatSpring.SetInitialValue(FloatSpring.CurrentValue);
			FloatSpring.TargetValue = FloatSpring.CurrentValue;
		}

		protected virtual void ApplyValue(float newValue)
		{
			TargetFloat = newValue;
		}
		
		protected override void GrabCurrentValue()
		{
			base.GrabCurrentValue();
			FloatSpring.CurrentValue = TargetFloat;
		}

		#endregion

		#region EVENTS
		
		public void OnMMEvent(MMSpringFloatEvent springEvent)
		{
			bool eventMatch = springEvent.ChannelData != null && MMChannel.Match(springEvent.ChannelData, ChannelMode, Channel, MMChannelDefinition);
			bool targetMatch = springEvent.TargetSpring != null && springEvent.TargetSpring.Equals(this);
			if (!eventMatch && !targetMatch)
			{
				return;
			}
			
			if (springEvent.OverrideDamping)
			{
				FloatSpring.Damping = springEvent.NewDamping;
			}
			if (springEvent.OverrideFrequency)
			{
				FloatSpring.Frequency = springEvent.NewFrequency;
			}

			switch (springEvent.Command)
			{
				case SpringCommands.MoveTo:
					MoveTo(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToAdditive:
					MoveToAdditive(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToSubtractive:
					MoveToSubtractive(springEvent.MoveToValue);
					break;
				case SpringCommands.MoveToRandom:
					MoveToRandom(springEvent.MoveToRandomValue.x, springEvent.MoveToRandomValue.y);
					break;
				case SpringCommands.MoveToInstant:
					MoveToInstant(springEvent.MoveToValue);
					break;
				case SpringCommands.Bump:
					Bump(springEvent.BumpAmount);
					break;
				case SpringCommands.BumpRandom:
					BumpRandom(springEvent.BumpAmountRandomValue.x, springEvent.BumpAmountRandomValue.y);
					break;
				case SpringCommands.Stop:
					Stop();
					break;
				case SpringCommands.Finish:
					Finish();
					break;
				case SpringCommands.RestoreInitialValue:
					RestoreInitialValue();
					break;
				case SpringCommands.ResetInitialValue:
					ResetInitialValue();
					break;
			}
		}
		
		protected override void Awake()
		{
			if (Target == null)
			{
				Target = GetComponent<T>();
			}
			base.Awake();
			this.MMEventStartListening<MMSpringFloatEvent>();
		}

		protected void OnDestroy()
		{
			this.MMEventStopListening<MMSpringFloatEvent>();
		}

		#endregion

		#region TEST_METHODS

		protected override void TestMoveTo()
		{
			MoveTo(TestMoveToValue);
		}
		
		protected override void TestMoveToAdditive()
		{
			MoveToAdditive(TestMoveToValue);
		}
		
		protected override void TestMoveToSubtractive()
		{
			MoveToSubtractive(TestMoveToValue);
		}
		
		protected override void TestMoveToRandom()
		{
			MoveToRandom();
		}

		protected override void TestMoveToInstant()
		{
			MoveToInstant(TestMoveToValue);
		}

		protected override void TestBump()
		{
			Bump(TestBumpAmount);
		}
		
		protected override void TestBumpRandom()
		{
			BumpRandom();
		}

		#endregion
	}
}
