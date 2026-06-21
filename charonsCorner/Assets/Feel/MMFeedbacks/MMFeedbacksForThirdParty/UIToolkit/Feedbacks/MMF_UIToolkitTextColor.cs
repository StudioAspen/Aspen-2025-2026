using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the text color an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the text color an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Text Color")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitTextColor : MMF_UIToolkitColorBase
	{
		protected override void ApplyColor(Color newColor)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.style.color = newColor;
				HandleMarkDirty(element);
			}
		}

		protected override Color GetInitialColor()
		{
			return _visualElements[0].resolvedStyle.color;
		}
	}
}