using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// An event used to pilot a MMSpringColor component
	/// </summary>
	public struct MMSpringColorEvent
	{
		static MMSpringColorEvent e;
		
		public MMChannelData ChannelData;
		public MMSpringComponentBase TargetSpring;
		public SpringCommands Command;
		public Color MoveToValue;
		public Color BumpAmount;
		public Color MoveToRandomValueMin;
		public Color MoveToRandomValueMax;
		public Color BumpAmountRandomValueMin;
		public Color BumpAmountRandomValueMax;
		public bool OverrideDamping;
		public float NewDamping;
		public bool OverrideFrequency;
		public float NewFrequency;
		
		public static void Trigger(SpringCommands command, MMSpringComponentBase targetSpring, MMChannelData channelData, 
			Color moveToValue = default, Color bumpAmount = default,
			Color moveToRandomValueMin = default, Color moveToRandomValueMax = default,
			Color bumpAmountRandomValueMin = default, Color bumpAmountRandomValueMax = default,
			bool overrideDamping = false, float newDamping = default, bool overrideFrequency = false, float newFrequency = default)
		{
			e.ChannelData = channelData;
			e.TargetSpring = targetSpring;
			e.Command = command;
			e.MoveToValue = moveToValue;
			e.BumpAmount = bumpAmount;
			e.MoveToRandomValueMin = moveToRandomValueMin;
			e.MoveToRandomValueMax = moveToRandomValueMax;
			e.BumpAmountRandomValueMin = bumpAmountRandomValueMin;
			e.BumpAmountRandomValueMax = bumpAmountRandomValueMax;
			e.OverrideDamping = overrideDamping;
			e.NewDamping = newDamping;
			e.OverrideFrequency = overrideFrequency;
			e.NewFrequency = newFrequency;
			MMEventManager.TriggerEvent(e);
		}
	}	
	
	/// <summary>
	/// A spring component used to pilot color values on a target
	/// </summary>
	public abstract class MMSpringColorComponent<T> : MMSpringComponentBase, MMEventListener<MMSpringColorEvent> where T:Component
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
		[Header("Spring")]
		/// the spring definition driving all sub spring components for this color spring
		[Tooltip("the spring definition driving all sub spring components for this color spring")]
		public MMSpringColor ColorSpring = new MMSpringColor();
		/// the multiplier to apply when bumping this color spring (increase this if you're not getting enough of the bump color on bump)
		[Tooltip("the multiplier to apply when bumping this color spring (increase this if you're not getting enough of the bump color on bump)")]
		public float BumpMultiplier = 20f;
		
		[MMInspectorGroup("Randomness", true, 12, true)]
		
		[Header("Move To Random")]
		
		/// the min color from which to pick a random color in MoveToRandom mode
		[Tooltip("the min color from which to pick a random color in MoveToRandom mode")]
		public Color MoveToRandomColorMin = MMColors.LawnGreen;
		/// the max color from which to pick a random color in MoveToRandom mode
		[Tooltip("the max color from which to pick a random color in MoveToRandom mode")]
		public Color MoveToRandomColorMax = MMColors.MediumSeaGreen;
		
		/// the min color from which to pick a random color in BumpRandom mode
		[Tooltip("the min color from which to pick a random color in BumpRandom mode")]
		public Color BumpRandomColorMin = MMColors.HotPink;
		/// the max color from which to pick a random color in BumpRandom mode
		[Tooltip("the max color from which to pick a random color in BumpRandom mode")]
		public Color BumpRandomColorMax = MMColors.Plum;
		
		[MMInspectorGroup("Test", true, 20, true)]
		/// the value to move this spring to when interacting with any of the MoveTo debug buttons in its inspector
		[Tooltip("the value to move this spring to when interacting with any of the MoveTo debug buttons in its inspector")]
		public Color TestMoveToColor = MMColors.Aquamarine;
		[MMInspectorButtonBar(new string[] { "MoveTo", "MoveToAdditive", "MoveToSubtractive", "MoveToRandom", "MoveToInstant" }, 
			new string[] { "TestMoveTo", "TestMoveToAdditive", "TestMoveToSubtractive", "TestMoveToRandom", "TestMoveToInstant" }, 
			new bool[] { true, true, true, true, true },
			new string[] { "main-call-to-action", "", "", "", "" })]
		public bool MoveToToolbar;
		
		/// the amount by which to bump this spring when interacting with the Bump debug button in its inspector
		[Tooltip("the amount by which to bump this spring when interacting with the Bump debug button in its inspector")]
		public Color TestBumpColor = MMColors.Orange;
		
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
		
		public override bool LowVelocity => (Mathf.Abs(ColorSpring.Velocity.r) + Mathf.Abs(ColorSpring.Velocity.g) + Mathf.Abs(ColorSpring.Velocity.b) + Mathf.Abs(ColorSpring.Velocity.a) + Mathf.Abs(ColorSpring.ColorSpring.Velocity)) < _velocityLowThreshold;
		public float DeltaTime => (TimeScaleMode == TimeScaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime;
		
		public virtual Color TargetColor { get; set; }
		
		protected bool _bumping = false;
		protected Color _newBumpColor;
		protected Color _bumpTargetColor;
		protected Color _initialBumpColor;
		protected Coroutine _coroutine;

		#region PUBLIC_API
		
		public virtual void MoveTo(Color newColor)
		{
			Activate();
			ColorSpring.MoveTo(newColor);
		}
		
		public virtual void MoveToAdditive(Color newValue)
		{
			Activate();
			ColorSpring.MoveToAdditive(newValue);
		}
		
		public virtual void MoveToSubtractive(Color newValue)
		{
			Activate();
			ColorSpring.MoveToSubtractive(newValue);
		}

		public virtual void MoveToRandom()
		{
			Activate();
			ColorSpring.MoveToRandom(MoveToRandomColorMin, MoveToRandomColorMax);
		}

		public virtual void MoveToInstant(Vector4 newValue)
		{
			Activate();
			ColorSpring.MoveToInstant(newValue);
		}

		public virtual void MoveToRandom(Color min, Color max)
		{
			Activate();
			ColorSpring.MoveToRandom(min, max);
		}
		
		public virtual void Bump(Color bumpColor)
		{
			Activate();
			_bumping = true;
			_bumpTargetColor = bumpColor;
			_initialBumpColor = ColorSpring.CurrentValue;
			ColorSpring.Bump(bumpColor);
		}

		public virtual void BumpRandom()
		{
			Activate();
			_bumpTargetColor = _bumpTargetColor.MMRandomColor(BumpRandomColorMin, BumpRandomColorMax);
			Bump(_bumpTargetColor);
		}

		public virtual void BumpRandom(Color min, Color max)
		{
			Activate();
			_bumpTargetColor = _bumpTargetColor.MMRandomColor(min, max);
			Bump(_bumpTargetColor);
		}
		
		public override void Stop()
		{
			base.Stop();
			this.enabled = false;
			GrabCurrentValue();
			ColorSpring.Stop();
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}
		
		public override void RestoreInitialValue()
		{
			ColorSpring.RestoreInitialValue();
			ApplyValue(ColorSpring.CurrentValue);
		}

		public override void ResetInitialValue()
		{
			ColorSpring.SetCurrentValueAsInitialValue();
		}
		
		protected override void UpdateSpringValue()
		{
			if (_bumping)
			{
				float t = ColorSpring.ColorSpring.CurrentValue * BumpMultiplier;
				ColorSpring.UpdateSpringValue(DeltaTime);
				_newBumpColor = Color.Lerp(_initialBumpColor, _bumpTargetColor, t);
				ApplyValue(_newBumpColor);
			}
			else
			{
				ColorSpring.UpdateSpringValue(DeltaTime);
				ApplyValue(ColorSpring.CurrentValue);	
			}
		}
		
		public override void Finish()
		{
			_bumping = false;
			ColorSpring.Finish();
			ApplyValue(ColorSpring.CurrentValue);
		}
		
		#endregion

		#region INTERNAL
		
		protected override void Initialization()
		{
			base.Initialization();
			GrabCurrentValue();
			ColorSpring.SetInitialValue(ColorSpring.CurrentValue);
			ColorSpring.TargetValue = ColorSpring.CurrentValue;
		}
		
		protected override void GrabCurrentValue()
		{
			base.GrabCurrentValue();
			ColorSpring.CurrentValue = TargetColor;
			
		}

		protected virtual void ApplyValue(Color newColor)
		{
			TargetColor = newColor;
		}

		/*protected virtual void ReplicateDriverSpring()
		{
			_springX.Damping = ColorSpring.Damping;
			_springY.Damping = ColorSpring.Damping;
			_springZ.Damping = ColorSpring.Damping;
			_springW.Damping = ColorSpring.Damping;
			_springX.Frequency = ColorSpring.Frequency;
			_springY.Frequency = ColorSpring.Frequency;
			_springZ.Frequency = ColorSpring.Frequency;
			_springW.Frequency = ColorSpring.Frequency;
		}*/

		#endregion
		
		#region EVENTS
		
		public void OnMMEvent(MMSpringColorEvent springEvent)
		{
			bool eventMatch = springEvent.ChannelData != null && MMChannel.Match(springEvent.ChannelData, ChannelMode, Channel, MMChannelDefinition);
			bool targetMatch = springEvent.TargetSpring != null && springEvent.TargetSpring.Equals(this);
			if (!eventMatch && !targetMatch)
			{
				return;
			}
			
			if (springEvent.OverrideDamping)
			{
				ColorSpring.SetDamping(springEvent.NewDamping);
			}
			if (springEvent.OverrideFrequency)
			{
				ColorSpring.SetFrequency(springEvent.NewFrequency);
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
					MoveToRandom(springEvent.MoveToRandomValueMin, springEvent.MoveToRandomValueMax);
					break;
				case SpringCommands.MoveToInstant:
					MoveToInstant(springEvent.MoveToValue);
					break;
				case SpringCommands.Bump:
					Bump(springEvent.BumpAmount);
					break;
				case SpringCommands.BumpRandom:
					BumpRandom(springEvent.BumpAmountRandomValueMin, springEvent.BumpAmountRandomValueMax);
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
			this.MMEventStartListening<MMSpringColorEvent>();
		}

		protected void OnDestroy()
		{
			this.MMEventStopListening<MMSpringColorEvent>();
		}
		
		#endregion

		#region TEST_METHODS

		protected override void TestMoveTo()
		{
			MoveTo(TestMoveToColor);
		}
		
		protected override void TestMoveToAdditive()
		{
			MoveToAdditive(TestMoveToColor);
		}
		
		protected override void TestMoveToSubtractive()
		{
			MoveToSubtractive(TestMoveToColor);
		}
		
		protected override void TestMoveToRandom()
		{
			MoveToRandom();
		}

		protected override void TestMoveToInstant()
		{
			MoveToInstant(TestMoveToColor);
		}

		protected override void TestBump()
		{
			Bump(TestBumpColor);
		}
		
		protected override void TestBumpRandom()
		{
			BumpRandom();
		}

		#endregion
	}
}
