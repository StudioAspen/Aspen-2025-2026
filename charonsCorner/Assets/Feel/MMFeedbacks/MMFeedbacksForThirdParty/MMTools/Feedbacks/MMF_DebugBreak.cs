using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will force a break, pausing the editor
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will will force a break, pausing the editor")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[System.Serializable]
	[FeedbackPath("Debug/Break")]
	public class MMF_DebugBreak : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
        
		/// <summary>
		/// On Play we break
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			Debug.Break();
		}
	}
}