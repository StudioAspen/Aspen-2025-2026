using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback is a base for UI Toolkit feedbacks
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback is a base for UI Toolkit feedbacks")]
	public class MMF_UIToolkit : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetDocument == null); }
		public override string RequiredTargetText { get { return TargetDocument != null ? TargetDocument.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires a target UI Document, set one in the TargetDocument field below"; } }
		#endif

		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetDocument = FindAutomatedTarget<UIDocument>();
		public enum QueryModes { Name, Class }

		[MMFInspectorGroup("Target", true, 54, true)]
		/// the UI document on which to make modifications 
		[Tooltip("the UI document on which to make modifications")]
		public UIDocument TargetDocument;
		/// the way to perform the query, either via element name or via class 
		[Tooltip("the way to perform the query, either via element name or via class")]
		public QueryModes QueryMode = QueryModes.Name;
		/// the query to perform (replace this with your own element name or class)
		[Tooltip("the query to perform (replace this with your own element name or class)")]
		public string Query = "ButtonA";
		/// whether to mark the UI document dirty after the operation. Set this to true when making a change that requires a repaint such as when using generateVisualContent to render a mesh and the mesh data has now changed.
		[Tooltip("whether to mark the UI document dirty after the operation. Set this to true when making a change that requires a repaint such as when using generateVisualContent to render a mesh and the mesh data has now changed.")]
		public bool MarkDirty = false;
		
		protected List<VisualElement> _visualElements = new List<VisualElement>();

		/// <summary>
		/// On init we turn the Image off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			PerformQuery();
		}

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
		}

		/// <summary>
		/// Performs the query and sets _visualElements with the result
		/// </summary>
		protected virtual void PerformQuery()
		{
			if (TargetDocument == null)
			{
				Debug.LogWarning("[UI Toolkit] The UI Toolkit feedback on "+Owner.name+" doesn't have a TargetDocument, it won't work. You need to specify one in its inspector.");
				return;
			}
			switch (QueryMode)
			{
				case QueryModes.Name:
					_visualElements = TargetDocument.rootVisualElement.Query(Query).ToList();
					break;
				case QueryModes.Class:
					_visualElements = TargetDocument.rootVisualElement.Query(className: Query).ToList();
					break;
			}
		}
		
		protected virtual void HandleMarkDirty(VisualElement element)
		{
			if (MarkDirty)
			{
				element.MarkDirtyRepaint();
			}
		}

	}
}