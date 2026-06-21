using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the text an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the text an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Text")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitText : MMF_UIToolkit
	{
		[Header("Text")]
		/// the new text to set on the target object(s)
		[Tooltip("the new text to set on the target object(s)")]
		public string NewText = "";

		protected string _initialText;
		
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			SetValue(NewText);
		}

		protected virtual void SetValue(string newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				(element as TextElement).text = newValue;
				HandleMarkDirty(element);
			}
		}
		
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
			_initialText = GetInitialValue();
		}

		protected virtual string GetInitialValue()
		{
			return (_visualElements[0] as TextElement).text;
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
			SetValue(_initialText);
		}
	}
}