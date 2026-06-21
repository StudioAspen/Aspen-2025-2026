using System;
using UnityEngine;
#if MM_VISUALEFFECTGRAPH
using UnityEngine.VFX;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you apply basic controls to a target VisualEffect
	/// </summary>
	[AddComponentMenu("")]
	[System.Serializable]
	[FeedbackHelp("This feedback will let you apply basic controls to a target VisualEffect")]
	#if MM_VISUALEFFECTGRAPH
	[FeedbackPath("Particles/VisualEffect")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.VisualEffectGraph")]
	public class MMF_VisualEffect : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		
		[MMFInspectorGroup("Visual Effect", true, 41)]
		/// the duration for the player to consider. This won't impact your visual effect, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual visual effect, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your visual effect, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual visual effect, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;
		
		#if MM_VISUALEFFECTGRAPH
		
		/// the various modes to control the target visual effect
		public enum Modes { Play, Stop, Pause, Unpause, AdvanceOneFrame, Reinit, SetPlayRate, Simulate }
		
		/// the visual effect to control when playing this feedback
		[Tooltip("the visual effect to control when playing this feedback")]
		public VisualEffect TargetVisualEffect;
		/// the selected mode, the instruction to send to the target visual effect when playing this feedback
		[Tooltip("the selected mode, the instruction to send to the target visual effect when playing this feedback")]
		public Modes Mode = Modes.Play;
		/// when in SetPlayRate mode, the new play rate to apply
		[Tooltip("when in SetPlayRate mode, the new play rate to apply")]
		[MMFEnumCondition("Mode", (int)Modes.SetPlayRate)]
		public float NewPlayRate = 1f;
		/// when in Simulate mode, the delta time to use
		[Tooltip("when in Simulate mode, the delta time to use")]
		[MMFEnumCondition("Mode", (int)Modes.Simulate)]
		public float StepDeltaTime = 1f;
		/// when in Simulate mode, the number of steps to simulate
		[Tooltip("when in Simulate mode, the number of steps to simulate")]
		[MMFEnumCondition("Mode", (int)Modes.Simulate)]
		public uint StepCount = 5;
		/// whether or not to stop the visual effect when stopping this feedback
		[Tooltip("whether or not to stop the visual effect when stopping this feedback")] 
		public bool StopVisualEffectOnStopFeedback = false;
		/// whether or not to stop the visual effect when resetting this feedback
		[Tooltip("whether or not to stop the visual effect when resetting this feedback")] 
		public bool StopVisualEffectOnReset = false;
		/// whether or not to stop the visual effect when initializing this feedback
		[Tooltip("whether or not to stop the visual effect when initializing this feedback")] 
		public bool StopVisualEffectOnInit = false;

		protected VFXEventAttribute _eventAttribute;

		/// <summary>
		/// On init we stop our visual effect if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			
			if (StopVisualEffectOnInit)
			{
				StopVisualEffect();
			}
		}

		/// <summary>
		/// On play we pass the selected instruction to our target visual effect
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetVisualEffect == null))
			{
				return;
			}

			switch (Mode)
			{
				case Modes.Play:
					TargetVisualEffect.Play();
					break;
				case Modes.Stop:
					StopVisualEffect();
					break;
				case Modes.Pause:
					TargetVisualEffect.pause = true;
					break;
				case Modes.Unpause:
					TargetVisualEffect.pause = false;
					break;
				case Modes.AdvanceOneFrame:
					TargetVisualEffect.AdvanceOneFrame();
					break;
				case Modes.Reinit:
					TargetVisualEffect.Reinit();
					break;
				case Modes.SetPlayRate:
					TargetVisualEffect.playRate = NewPlayRate;
					break;
				case Modes.Simulate:
					TargetVisualEffect.Simulate(StepDeltaTime, StepCount);
					break;
			}
		}
		
		/// <summary>
		/// On stop we stop our visual effect if needed
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

			if (StopVisualEffectOnStopFeedback)
			{
				StopVisualEffect();
			}
		}

		/// <summary>
		/// On Reset, stops the visual effect if needed
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (StopVisualEffectOnReset)
			{
				StopVisualEffect();
			}
		}

		/// <summary>
		/// Stops the target visual effect
		/// </summary>
		protected virtual void StopVisualEffect()
		{
			if (TargetVisualEffect == null)
			{
				return;
			}
			
			TargetVisualEffect.Stop();
		}
		#else
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f) { }
		#endif
	}
}