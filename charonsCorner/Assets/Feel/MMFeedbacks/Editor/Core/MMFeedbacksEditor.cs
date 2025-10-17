using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A custom editor displaying a foldable list of MMFeedbacks, a dropdown to add more, as well as test buttons to test your feedbacks at runtime
	/// </summary>
	[CustomEditor(typeof(MMFeedbacks))]
	public class MMFeedbacksEditor : Editor
	{
		/// <summary>
		/// Draws the inspector, complete with helpbox, init mode selection, list of feedbacks, feedback selection and test buttons 
		/// </summary>
		public override void OnInspectorGUI()
		{
			
				EditorGUILayout.HelpBox("The MMFeedbacks component got deprecated with the introduction of the MMF Player, in v3.0.\n\n" +
				                        "The MMF Player improves performance, lets you keep runtime changes, and much more! And it works just like MMFeedbacks. " +
				                        "With the release of v4.0, the MMFeedbacks is now completely removed from Feel and phased out.\n\n" +
				                        "If you've tried adding this component, maybe you're watching an old tutorial, in that case, fear not, all you're watching is still valid, " +
				                        "just replace MMFeedbacks with MMF Player and you'll be good to go! Have fun with Feel!", MessageType.Warning);  
		}
	}
}