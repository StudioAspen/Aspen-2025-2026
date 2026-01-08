using UnityEngine;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the sprite of a target SpriteRenderer
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the sprite of a target SpriteRenderer.")]
	[FeedbackPath("Renderer/Sprite")]
	public class MMF_Sprite : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundSpriteRenderer == null); }
		public override string RequiredTargetText { get { return BoundSpriteRenderer != null ? BoundSpriteRenderer.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundSpriteRenderer be set to be able to work properly. You can set one below."; } }
		#endif

		public override float FeedbackDuration => 0f;
		public override bool HasChannel => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundSpriteRenderer = FindAutomatedTarget<SpriteRenderer>();

		[MMFInspectorGroup("Sprite", true, 54, true)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("the SpriteRenderer to affect when playing the feedback")]
		public SpriteRenderer BoundSpriteRenderer;
		/// the Sprite to apply to the BoundSpriteRenderer when this feedback plays
		[Tooltip("the Sprite to apply to the BoundSpriteRenderer when this feedback plays")]
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
				if (BoundSpriteRenderer == null)
				{
					Debug.LogWarning("[Sprite Feedback] The Sprite feedback on "+Owner.name+" doesn't have a BoundSpriteRenderer, it won't work. You need to specify a Sprite Renderer in its inspector.");
				}
				else
				{
					_initialSprite = BoundSpriteRenderer.sprite;
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
			if (!Active || !FeedbackTypeAuthorized || (BoundSpriteRenderer == null))
			{
				return;
			}
			
			SetSprite(NormalPlayDirection ? NewSprite : _initialSprite);
		}

		/// <summary>
		/// Sets the sprite on the BoundSpriteRenderer
		/// </summary>
		/// <param name="newSprite"></param>
		protected virtual void SetSprite(Sprite newSprite)
		{
			BoundSpriteRenderer.sprite = newSprite;
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