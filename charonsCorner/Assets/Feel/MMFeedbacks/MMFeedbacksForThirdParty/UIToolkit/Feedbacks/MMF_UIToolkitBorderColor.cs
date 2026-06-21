using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the border color of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the border color of an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Border Color")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitBorderColor : MMF_UIToolkitColorBase
	{
		[MMFInspectorGroup("Borders", true, 55, true)]
		/// whether or not the feedback should modify the color of the left border
		[Tooltip("whether or not the feedback should modify the color of the left border")]
		public bool BorderLeft = true;
		/// whether or not the feedback should modify the color of the right border
		[Tooltip("whether or not the feedback should modify the color of the right border")]
		public bool BorderRight = true;
		/// whether or not the feedback should modify the color of the bottom border
		[Tooltip("whether or not the feedback should modify the color of the bottom border")]
		public bool BorderBottom = true;
		/// whether or not the feedback should modify the color of the top border
		[Tooltip("whether or not the feedback should modify the color of the top border")]
		public bool BorderTop = true;
		
		protected override void ApplyColor(Color newColor)
		{
			foreach (VisualElement element in _visualElements)
			{
				if (BorderLeft)
				{
					element.style.borderLeftColor = newColor;
				}
				if (BorderRight)
				{
					element.style.borderRightColor = newColor;
				}
				if (BorderBottom)
				{
					element.style.borderBottomColor = newColor;
				}
				if (BorderTop)
				{
					element.style.borderTopColor = newColor;
				}
				HandleMarkDirty(element);
			}
		}

		protected override Color GetInitialColor()
		{
			if (BorderLeft)
			{
				return _visualElements[0].resolvedStyle.borderLeftColor;
			}
			if (BorderRight)
			{
				return _visualElements[0].resolvedStyle.borderRightColor;
			}
			if (BorderBottom)
			{
				return _visualElements[0].resolvedStyle.borderBottomColor;
			}
			if (BorderTop)
			{
				return _visualElements[0].resolvedStyle.borderTopColor;
			}
			return Color.black;
		}
	}
}