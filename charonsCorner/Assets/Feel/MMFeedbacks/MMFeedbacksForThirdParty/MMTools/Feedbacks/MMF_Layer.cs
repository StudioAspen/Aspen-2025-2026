using System.Collections;
using UnityEngine;
#if MM_UI
using MoreMountains.Tools;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the layer of a target game object when playing the feedback
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the layer of a target game object when playing the feedback")]
	[System.Serializable]
	[FeedbackPath("GameObject/Layer")]
	public class MMF_Layer : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetGameObject == null); }
		public override string RequiredTargetText { get { return TargetGameObject != null ? TargetGameObject.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetGameObject be set to be able to work properly. You can set one below."; } }
		#endif

		/// the duration of this feedback is the duration of the Graphic, or 0 if instant
		public override float FeedbackDuration { get { return 0f; }  }
		public override bool HasChannel => false;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetGameObject = FindAutomatedTarget<GameObject>();

		[MMFInspectorGroup("Graphic", true, 54, true)]
		/// the game object you want to change the layer on
		[Tooltip("the game object you want to change the layer on")]
		public GameObject TargetGameObject;
		/// the new layer to assign to the target game object 
		[Tooltip("the new layer to assign to the target game object")]
		[MMLayer]
		public int NewLayer;
		
		protected int _initialLayer;

		/// <summary>
		/// On init we store the initial layer
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active && TargetGameObject != null)
			{
				_initialLayer = TargetGameObject.layer;
			}
		}

		/// <summary>
		/// On Play we change our object's layer to the New Layer
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
			TargetGameObject.layer = NewLayer;
		}
		
		/// <summary>
		/// On restore, we restore our initial layer
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetGameObject.layer = _initialLayer;
		}
	}
}
#endif