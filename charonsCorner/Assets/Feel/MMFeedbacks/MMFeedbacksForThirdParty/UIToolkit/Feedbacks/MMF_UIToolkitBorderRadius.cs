using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the border radius of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the border radius of an element on a target UI Document")]
	[FeedbackPath("UI Toolkit/UITK Border Radius")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitBorderRadius : MMF_UIToolkitFloatBase
	{
		/// whether to modify the bottom left border radius or not
		[Tooltip("whether to modify the bottom left border radius or not")]
		public bool BottomLeft = true;
		/// whether to modify the bottom right border radius or not
		[Tooltip("whether to modify the bottom right border radius or not")]
		public bool BottomRight = true;
		/// whether to modify the top left border radius or not
		[Tooltip("whether to modify the top left border radius or not")]
		public bool TopLeft = true;
		/// whether to modify the top right border radius or not
		[Tooltip("whether to modify the top right border radius or not")]
		public bool TopRight = true;
		
		protected override void SetValue(float newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				if (BottomLeft) element.style.borderBottomLeftRadius = newValue;
				if (BottomRight) element.style.borderBottomRightRadius = newValue;
				if (TopLeft) element.style.borderTopLeftRadius = newValue;
				if (TopRight) element.style.borderTopRightRadius = newValue;
				HandleMarkDirty(element);
			}
		}

		protected override float GetInitialValue()
		{
			if (BottomLeft) return _visualElements[0].resolvedStyle.borderBottomLeftRadius;
			if (BottomRight) return _visualElements[0].resolvedStyle.borderBottomRightRadius;
			if (TopLeft) return _visualElements[0].resolvedStyle.borderTopLeftRadius;
			if (TopRight) return _visualElements[0].resolvedStyle.borderTopRightRadius;
			return _visualElements[0].resolvedStyle.borderBottomLeftRadius;
		}
	}
}