using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you set the visibility of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you set the visibility of an element on a target UI Document")]
	[FeedbackPath("UI Toolkit/UITK Visible")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitVisible : MMF_UIToolkitBoolBase
	{
		public enum Modes { Set, Toggle }
		
		[Header("Visible")]
		/// the selected mode (set : sets the object visible or not, toggle : toggles the object's visibility)
		[Tooltip("the selected mode (set : sets the object visible or not, toggle : toggles the object's visibility)")]
		public Modes Mode = Modes.Set;
		/// whether to set the object visible (true) or not
		[Tooltip("whether to set the object visible (true) or not")]
		[MMFEnumCondition("Mode", (int)Modes.Set)]
		public bool Visible = false;
		
		protected override void SetValue()
		{
			foreach (VisualElement element in _visualElements)
			{
				switch (Mode)
				{
					case Modes.Set:
						element.visible = Visible;
						break;
					case Modes.Toggle:
						element.visible = !element.visible;
						break;
				}
				HandleMarkDirty(element);
			}
		}
		
		protected override void SetValue(bool newValue)
		{
			foreach (VisualElement element in _visualElements)
			{
				element.visible = newValue;
				HandleMarkDirty(element);
			}
		}

		protected override bool GetInitialValue()
		{
			return _visualElements[0].visible;
		}
	}
}