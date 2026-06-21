using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	[System.Serializable]
	public class WeightedEvent
	{
		public int Weight;
		public UnityEvent Event;
	}
	
	/// <summary>
	/// This feedback allows you to play a random Unity Event, out of a weighted list. To use it, add items to its WeightedEvents list. For each of them, you'll need to specify a weight (the higher the weight, the more likely it'll be picked) and the event to trigger. For an event in that list to have a chance to be picked, the weights can't be zero.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback allows you to play a random Unity Event, out of a weighted list. To use it, add items to its WeightedEvents list. For each of them, you'll need to specify a weight (the higher the weight, the more likely it'll be picked) and the event to trigger. For an event in that list to have a chance to be picked, the weights can't be zero.")]
	[System.Serializable]
	[FeedbackPath("Events/Random Unity Events")]
	public class MMF_RandomEvents : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif

		[MMFInspectorGroup("Events", true, 44)]
		/// the list of events from which to pick 
		[Tooltip("the list of events from which to pick")]
		public List<WeightedEvent> WeightedEvents;
		
		protected MMShufflebag<int> _weightShuffleBag;

		/// <summary>
		/// On init, triggers the init events
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if ((WeightedEvents == null) || (WeightedEvents.Count == 0))
			{
				return;
			}
			_weightShuffleBag = new MMShufflebag<int>(WeightedEvents.Count);
			for (var index = 0; index < WeightedEvents.Count; index++)
			{
				_weightShuffleBag.Add(index, WeightedEvents[index].Weight);
			}
		}

		/// <summary>
		/// On Play, triggers the play events
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if ((WeightedEvents == null) || (WeightedEvents.Count == 0) || (_weightShuffleBag == null))
			{
				return;
			}

			int newIndex = _weightShuffleBag.Pick();
			WeightedEvents[newIndex].Event.Invoke();
		}
	}
}