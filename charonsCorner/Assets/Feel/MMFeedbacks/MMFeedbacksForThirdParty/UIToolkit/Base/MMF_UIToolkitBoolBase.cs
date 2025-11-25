using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A base feedback to set a bool on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_UIToolkitBoolBase : MMF_UIToolkit
	{
		/// the duration of this feedback is the duration of the color transition, or 0 if instant
		public override float FeedbackDuration { get { return 0f; }}
		public override bool HasCustomInspectors => true;
		
		protected bool _initialValue;

		/// <summary>
		/// On init we store our initial value
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
			_initialValue = GetInitialValue();
		}

		/// <summary>
		/// On Play we change our text's alpha
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
			
			SetValue();
		}

		/// <summary>
		/// Stops the animation if needed
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
			IsPlaying = false;
		}

		protected virtual void SetValue()
		{
			
		}
		protected virtual void SetValue(bool newValue)
		{
			
		}

		protected virtual bool GetInitialValue()
		{
			return false;
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
			SetValue(_initialValue);
		}
	}
}