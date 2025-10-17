using System.Collections;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback will let you change the class of an element on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the class of an element on a target UI Document")]
	[FeedbackPath("UI Toolkit/UITK Class")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.UIToolkit")]
	public class MMF_UIToolkitClass : MMF_UIToolkit
	{
		public enum Modes { AddToClassList, EnableInClassList, ToggleInClassList, RemoveFromClassList, ClearClassList}

		[Header("Class Manipulation")] 
		/// whether to add, enable, toggle, remove or clear the class list
		[Tooltip("whether to add, enable, toggle, remove or clear the class list")]
		public Modes Mode = Modes.AddToClassList;
		/// the name of the class to add, enable, toggle or remove
		[Tooltip("the name of the class to add, enable, toggle or remove")]
		[MMFEnumCondition("Mode", (int)Modes.AddToClassList, (int)Modes.EnableInClassList, (int)Modes.ToggleInClassList, (int)Modes.RemoveFromClassList)]
		public string ClassName = "";
		/// in EnableInClassList mode, whether to enable or disable the class
		[Tooltip("in EnableInClassList mode, whether to enable or disable the class")]
		[MMFEnumCondition("Mode", (int)Modes.EnableInClassList)]
		public bool Enable = true;
		
		
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			foreach (VisualElement element in _visualElements)
			{
				switch (Mode)
				{
					case Modes.AddToClassList:
						element.AddToClassList(ClassName);
						break;
					case Modes.EnableInClassList:
						element.EnableInClassList(ClassName, Enable);
						break;
					case Modes.ToggleInClassList:
						element.ToggleInClassList(ClassName);
						break;
					case Modes.RemoveFromClassList:
						element.RemoveFromClassList(ClassName);
						break;
					case Modes.ClearClassList:
						element.ClearClassList();
						break;
				}
				HandleMarkDirty(element);
			}
		}
	}
}