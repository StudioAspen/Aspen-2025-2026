#if MM_UI
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the sprite of a target Image
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the sprite of a target Image.")]
	[FeedbackPath("UI/Image Sprite")]
	public class MMF_ImageSprite : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundImage == null); }
		public override string RequiredTargetText { get { return BoundImage != null ? BoundImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundImage be set to be able to work properly. You can set one below."; } }
		#endif

		public override float FeedbackDuration => 0f;
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundImage = FindAutomatedTarget<Image>();

		/// the possible modes for this feedback
		public enum Modes { Sprite, OverrideSprite }

		[MMFInspectorGroup("Image", true, 54, true)]
		/// the Image to affect when playing the feedback
		[Tooltip("the Image to affect when playing the feedback")]
		public Image BoundImage;
		/// whether to target the Image's Sprite or OverrideSprite to replace it
		[Tooltip("whether to target the Image's Sprite or OverrideSprite to replace it")]
		public Modes Mode = Modes.Sprite;
		/// the Sprite to apply to the BoundImage when this feedback plays
		[Tooltip("the Sprite to apply to the BoundImage when this feedback plays")]
		public Sprite NewSprite;
		
		protected Sprite _initialSprite;

		/// <summary>
		/// On init we store our initial sprite
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (BoundImage == null)
				{
					Debug.LogWarning("[Image Sprite Feedback] The Image Sprite feedback on "+Owner.name+" doesn't have a BoundImage, it won't work. You need to specify an Image in its inspector.");
				}
				else
				{
					_initialSprite = BoundImage.sprite;
				}
			}
		}

		/// <summary>
		/// On Play we change our sprite
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (BoundImage == null))
			{
				return;
			}
			
			SetSprite(NormalPlayDirection ? NewSprite : _initialSprite);
		}

		/// <summary>
		/// Sets the sprite on the BoundImage
		/// </summary>
		/// <param name="newSprite"></param>
		protected virtual void SetSprite(Sprite newSprite)
		{
			switch (Mode)
			{
				case Modes.Sprite:
					BoundImage.sprite = newSprite;
					break;
				case Modes.OverrideSprite:
					BoundImage.overrideSprite = newSprite;
					break;
			}
		}

		/// <summary>
		/// Called on stop
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
		}
		
		/// <summary>
		/// On restore, we restore our initial sprite
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			SetSprite(_initialSprite);
		}
	}
}
#endif