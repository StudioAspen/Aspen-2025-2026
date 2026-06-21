using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will move the current "head" of an MMF_Player sequence back to another feedback above in the list.
	/// What feedback the head lands on depends on your settings : you can decide to have it loop at last pause, or at the last LoopStart feedback in the list (or both).
	/// Furthermore, you can decide to have it loop multiple times and cause a pause when met.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will move the current 'head' of an MMF_Player sequence back to another feedback above in the list. " +
	              "What feedback the head lands on depends on your settings : you can decide to have it loop at last pause, " +
	              "or at the last LoopStart feedback in the list (or both). Furthermore, you can decide to have it loop multiple times and cause a pause when met.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Loop/Looper")]
	public class MMF_Looper : MMF_Pause
	{
		[MMFInspectorGroup("Loop", true, 34)]
        
		[Header("Loop conditions")]
		/// if this is true, this feedback, when met, will cause the MMF_Player to reposition its 'head' to the first pause found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("if this is true, this feedback, when met, will cause the MMF_Player to reposition its 'head' to the first pause found above it (going from this feedback to the top), or to the start if none is found")]
		public bool LoopAtLastPause = true;
		/// if this is true, this feedback, when met, will cause the MMF_Player to reposition its 'head' to the first LoopStart feedback found above it (going from this feedback to the top), or to the start if none is found
		[Tooltip("if this is true, this feedback, when met, will cause the MMF_Player to reposition its 'head' to the first LoopStart feedback found above it (going from this feedback to the top), or to the start if none is found")]
		public bool LoopAtLastLoopStart = true;

		[Header("Loop")]
		/// if this is true, the looper will loop forever
		[Tooltip("if this is true, the looper will loop forever")]
		public bool InfiniteLoop = false;
		/// how many times this loop should run
		[Tooltip("how many times this loop should run")]
		[MMCondition("InfiniteLoop", true, true)]
		public int NumberOfLoops = 2;
		/// the amount of loops left (updated at runtime)
		[Tooltip("the amount of loops left (updated at runtime)")]
		[MMFReadOnly]
		public int NumberOfLoopsLeft = 1;
		/// whether we are in an infinite loop at this time or not
		[Tooltip("whether we are in an infinite loop at this time or not")]
		[MMFReadOnly]
		public bool InInfiniteLoop = false;
		/// whether or not to trigger a Loop MMFeedbacksEvent when this looper is reached
		[Tooltip("whether or not to trigger a Loop MMFeedbacksEvent when this looper is reached")]
		public bool TriggerMMFeedbacksEvents = true;

		[Header("Events")] 
		/// a Unity Event to invoke when the looper is reached
		[Tooltip("a Unity Event to invoke when the looper is reached")]
		public UnityEvent OnLoop;

		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperColor; } }
		public override Color DisplayColor { get { return MMFeedbacksInspectorColors.LooperColor.MMDarken(0.25f); } }
		#endif
		public override bool LooperPause { get { return true; } }

		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// On init we initialize our number of loops left
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
			if (OnLoop == null)
			{
				OnLoop = new UnityEvent();
			}
		}

		/// <summary>
		/// On play we decrease our counter and play our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				ProcessNewPauseDuration();
				InInfiniteLoop = InfiniteLoop;
				NumberOfLoopsLeft--;
				Owner.StartCoroutine(PlayPause());
				TriggerOnLoop(Owner);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public virtual void TriggerOnLoop(MMFeedbacks source)
		{
			OnLoop.Invoke();

			if (TriggerMMFeedbacksEvents)
			{
				MMFeedbacksEvent.Trigger(source, MMFeedbacksEvent.EventTypes.Loop);
			}
		}

		/// <summary>
		/// On custom stop, we exit our infinite loop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			InInfiniteLoop = false;
		}

		/// <summary>
		/// On reset we reset our amount of loops left
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			InInfiniteLoop = InfiniteLoop;
			NumberOfLoopsLeft = NumberOfLoops;
		}
	}
}