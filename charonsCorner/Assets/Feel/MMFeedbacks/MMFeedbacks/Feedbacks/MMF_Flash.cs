using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
#if MM_UI
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a flash event (to be caught by a MMFlash) when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("On play, this feedback will broadcast a MMFlashEvent. If you create a UI image with a MMFlash component on it (see example in the Demo scene), it will intercept that event, and flash (usually you'll want it to take the full size of your screen, but that's not mandatory). In the feedback's inspector, you can define the color of the flash, its duration, alpha, and a FlashID. That FlashID needs to be the same on your feedback and MMFlash for them to work together. This allows you to have multiple MMFlashs in your scene, and flash them separately.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Camera/Flash")]
	public class MMF_Flash : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif
		/// the duration of this feedback is the duration of the flash
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlashDuration); } set { FlashDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Flash", true, 37)]
		/// the color of the flash
		[Tooltip("the color of the flash")]
		public Color FlashColor = Color.white;
		/// the flash duration (in seconds)
		[Tooltip("the flash duration (in seconds)")]
		public float FlashDuration = 0.2f;
		/// the alpha of the flash
		[Tooltip("the alpha of the flash")]
		public float FlashAlpha = 1f;
		/// the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)
		[Tooltip("the ID of the flash (usually 0). You can specify on each MMFlash object an ID, allowing you to have different flash images in one scene and call them separately (one for damage, one for health pickups, etc)")]
		public int FlashID = 0;

		[Header("Optional Target")] 
		/// this field lets you bind a specific MMFlash to this feedback. If left empty, the feedback will trigger a MMFlashEvent instead, targeting all matching flashes. If you fill it, only that specific MMFlash will be targeted.
		[Tooltip("this field lets you bind a specific MMFlash to this feedback. If left empty, the feedback will trigger a MMFlashEvent instead, targeting all matching flashes. If you fill it, only that specific MMFlash will be targeted.")]
		public MMFlash TargetFlash;

		/// <summary>
		/// On Play we trigger a flash event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (TargetFlash != null)
			{
				TargetFlash.Flash(FlashColor, FlashDuration * intensityMultiplier, FlashAlpha, ComputedTimescaleMode);
			}
			else
			{
				MMFlashEvent.Trigger(FlashColor, FeedbackDuration * intensityMultiplier, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode);	
			}
		}

		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
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
			MMFlashEvent.Trigger(FlashColor, FeedbackDuration, FlashAlpha, FlashID, ChannelData, ComputedTimescaleMode, stop:true);
		}
		
		/// <summary>
		/// Automatically tries to add a MMFlash setup to the scene
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			if (Object.FindAnyObjectByType<MMFlash>() != null)
			{
				return;
			}
			
			(Canvas canvas, bool createdNewCanvas) = Owner.gameObject.MMFindOrCreateObjectOfType<Canvas>("FlashCanvas", null);
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			(Image image, bool createdNewImage) = canvas.gameObject.MMFindOrCreateObjectOfType<Image>("FlashImage", canvas.transform, true);
			image.raycastTarget = false;
			image.color = Color.white;
			
			RectTransform rectTransform = image.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			
			image.gameObject.AddComponent<MMFlash>();
			image.gameObject.GetComponent<CanvasGroup>().alpha = 0;
			image.gameObject.GetComponent<CanvasGroup>().interactable = false;

			MMDebug.DebugLogInfo("Added a MMFlash to the scene. You're all set.");
		}
	}
}
#endif