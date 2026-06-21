using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the image tint of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the image tint of an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Image Tint")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitImageTint : MMF_UIToolkitColorBase
	{
		protected override void ApplyColor(Color newColor)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.style.unityBackgroundImageTintColor = newColor;
				HandleMarkDirty(element);
			}
		}

		protected override Color GetInitialColor()
		{
			return _visualElements[0].resolvedStyle.unityBackgroundImageTintColor;
		}
	}
}