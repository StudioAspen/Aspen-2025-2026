using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the stylesheet on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the stylesheet on a target UI Document")]
	[FeedbackPath("UI Toolkit/UITK Stylesheet")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitStylesheet : MMF_UIToolkit
	{
		[Header("Stylesheet")] 
		/// the new stylesheet to apply to the document
		[Tooltip("the new stylesheet to apply to the document")]
		public StyleSheet NewStylesheet;
		
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.styleSheets.Add(NewStylesheet);
				HandleMarkDirty(element);
			}
		}
	}
}