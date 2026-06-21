#if MM_UI
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the material on a target UI Image
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the material on a target UI Image")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("UI/Image Material")]
	public class MMF_ImageMaterial : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetImage be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();
        
		[MMFInspectorGroup("Image", true, 12, true)]
		/// the target Image we want to change the material on
		[Tooltip("the target Image we want to change the material on")]
		public Image TargetImage;
		
		/// the new material to apply to the target image
		[Tooltip("the new material to apply to the target image")]
		public Material NewMaterial;

		protected Material _initialMaterial;
		
		/// <summary>
		/// On play we turn raycastTarget on or off
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetImage == null)
			{
				return;
			}

			_initialMaterial = TargetImage.material;
			TargetImage.material = NewMaterial;
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetImage.material = _initialMaterial;
		}
	}
}
#endif