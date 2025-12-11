using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the border width of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the border width of an element on a target UI Document")]
	[FeedbackPath("UI Toolkit/UITK Border Width")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitBorderWidth : MMF_UIToolkitFloatBase
	{
		/// whether to modify the left border width or not
		[Tooltip("whether to modify the left border width or not")]
		public bool Left = true;
		/// whether to modify the right border width or not
		[Tooltip("whether to modify the right border width or not")]
		public bool Right = true;
		/// whether to modify the top border width or not
		[Tooltip("whether to modify the top border width or not")]
		public bool Top = true;
		/// whether to modify the bottom border width or not
		[Tooltip("whether to modify the bottom border width or not")]
		public bool Bottom = true;
		
		protected override void SetValue(float newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				if (Left) element.style.borderLeftWidth = newValue;
				if (Right) element.style.borderRightWidth = newValue;
				if (Bottom) element.style.borderBottomWidth = newValue;
				if (Top) element.style.borderTopWidth = newValue;
				HandleMarkDirty(element);
			}
		}

		protected override float GetInitialValue()
		{
			if (Left) return _visualElements[0].resolvedStyle.borderLeftWidth;
			if (Right) return _visualElements[0].resolvedStyle.borderRightWidth;
			if (Bottom) return _visualElements[0].resolvedStyle.borderBottomWidth;
			if (Top) return _visualElements[0].resolvedStyle.borderTopWidth;
			return _visualElements[0].resolvedStyle.borderLeftWidth;
		}
	}
}