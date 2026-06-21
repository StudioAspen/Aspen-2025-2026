using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the opacity of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the opacity of an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Opacity")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitOpacity : MMF_UIToolkitFloatBase
	{
		protected override void SetValue(float newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.style.opacity = newValue;
				HandleMarkDirty(element);
			}
		}

		protected override float GetInitialValue()
		{
			return _visualElements[0].resolvedStyle.opacity;
		}
	}
}