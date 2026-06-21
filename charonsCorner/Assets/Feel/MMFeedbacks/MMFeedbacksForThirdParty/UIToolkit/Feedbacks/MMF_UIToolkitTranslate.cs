using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you translate an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you translate an element on a target UI Document")]
	[System.Serializable]
	[FeedbackPath("UI Toolkit/UITK Translate")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitTranslate : MMF_UIToolkitVector2Base
	{
		[Header("Units")]
		/// how to interpret the x value
		[Tooltip("how to interpret the x value")]
		public LengthUnit LengthUnitX = LengthUnit.Pixel;
		/// how to interpret the y value
		[Tooltip("how to interpret the y value")]
		public LengthUnit LengthUnitY = LengthUnit.Pixel;
		
		protected override void SetValue(Vector2 newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.style.translate = new StyleTranslate(new Translate(new Length(newValue.x, LengthUnitX), new Length(newValue.y, LengthUnitY)));
				HandleMarkDirty(element);
			}
		}

		protected override Vector2 GetInitialValue()
		{
			return _visualElements[0].resolvedStyle.translate;
		}
	}
}