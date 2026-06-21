#if MM_UI
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a one time play on a target FloatController
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback lets you trigger a fade event.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[System.Serializable]
	[FeedbackPath("Camera/Fade")]
	public class MMF_Fade : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText { get { return "ID "+ID;  } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif
		/// the different possible types of fades
		public enum FadeTypes { FadeIn, FadeOut, Custom }
		/// the different ways to send the position to the fader :
		/// - FeedbackPosition : fade at the position of the feedback, plus an optional offset
		/// - Transform : fade at the specified Transform's position, plus an optional offset
		/// - WorldPosition : fade at the specified world position vector, plus an optional offset
		/// - Script : the position passed in parameters when calling the feedback
		public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

		[MMFInspectorGroup("Fade", true, 43)]
		/// the type of fade we want to use when this feedback gets played
		[Tooltip("the type of fade we want to use when this feedback gets played")]
		public FadeTypes FadeType;
		/// the ID of the fader(s) to pilot
		[Tooltip("the ID of the fader(s) to pilot")]
		public int ID = 0;
		/// the duration (in seconds) of the fade
		[Tooltip("the duration (in seconds) of the fade")]
		public float Duration = 1f;
		/// the curve to use for this fade
		[Tooltip("the curve to use for this fade")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// whether or not this fade should ignore timescale
		[Tooltip("whether or not this fade should ignore timescale")]
		public bool IgnoreTimeScale = true;

		[Header("Custom")]
		/// the target alpha we're aiming for with this fade
		[Tooltip("the target alpha we're aiming for with this fade")]
		public float TargetAlpha;

		[Header("Position")]
		/// the chosen way to position the fade 
		[Tooltip("the chosen way to position the fade")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the transform on which to center the fade
		[Tooltip("the transform on which to center the fade")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the coordinates on which to center the fadet
		[Tooltip("the coordinates on which to center the fade")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset to apply when centering the fade
		[Tooltip("the position offset to apply when centering the fade")]
		public Vector3 PositionOffset;

		[Header("Optional Target")] 
		/// this field lets you bind a specific MMFader to this feedback. If left empty, the feedback will trigger a MMFadeEvent instead, targeting all matching faders. If you fill it, only that specific fader will be targeted.
		[Tooltip("this field lets you bind a specific MMFader to this feedback. If left empty, the feedback will trigger a MMFadeEvent instead, targeting all matching faders. If you fill it, only that specific fader will be targeted.")]
		public MMFader TargetFader;

		/// the duration of this feedback is the duration of the fade
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }

		protected Vector3 _position;
		protected FadeTypes _fadeType;

		/// <summary>
		/// On play we trigger the selected fade event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_position = GetPosition(position);
			_fadeType = FadeType;
			if (!NormalPlayDirection)
			{
				if (FadeType == FadeTypes.FadeIn)
				{
					_fadeType = FadeTypes.FadeOut;
				}
				else if (FadeType == FadeTypes.FadeOut)
				{
					_fadeType = FadeTypes.FadeIn;
				}
			}

			if (TargetFader != null)
			{
				switch (_fadeType)
				{
					case FadeTypes.Custom:
						TargetFader.Fade(TargetAlpha, FeedbackDuration, Curve, IgnoreTimeScale);
						break;
					case FadeTypes.FadeIn:
						TargetFader.FadeIn(FeedbackDuration, Curve, IgnoreTimeScale);
						break;
					case FadeTypes.FadeOut:
						TargetFader.FadeOut(FeedbackDuration, Curve, IgnoreTimeScale);
						break;
				}
			}
			else
			{
				switch (_fadeType)
				{
					case FadeTypes.Custom:
						MMFadeEvent.Trigger(FeedbackDuration, TargetAlpha, Curve, ID, IgnoreTimeScale, _position);
						break;
					case FadeTypes.FadeIn:
						MMFadeInEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
						break;
					case FadeTypes.FadeOut:
						MMFadeOutEvent.Trigger(FeedbackDuration, Curve, ID, IgnoreTimeScale, _position);
						break;
				}
			}
		}

		/// <summary>
		/// Stops the animation if needed
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
			MMFadeStopEvent.Trigger(ID);
		}

		/// <summary>
		/// Computes the proper position for this fade
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + PositionOffset;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset;
				case PositionModes.Script:
					return position + PositionOffset;
				default:
					return position + PositionOffset;
			}
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
			MMFadeStopEvent.Trigger(ID, true);
		}
		
		/// <summary>
		/// Automatically tries to add a MMFader setup to the scene
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			if (Object.FindAnyObjectByType<MMFader>() != null)
			{
				return;
			}
			
			(Canvas canvas, bool createdNewCanvas) = Owner.gameObject.MMFindOrCreateObjectOfType<Canvas>("FadeCanvas", null);
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			(Image image, bool createdNewImage) = canvas.gameObject.MMFindOrCreateObjectOfType<Image>("FadeImage", canvas.transform, true);
			image.raycastTarget = false;
			image.color = Color.black;
			
			RectTransform rectTransform = image.GetComponent<RectTransform>();
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
			
			image.gameObject.AddComponent<MMFader>();
			image.gameObject.GetComponent<CanvasGroup>().alpha = 0;
			image.gameObject.GetComponent<CanvasGroup>().interactable = false;

			MMDebug.DebugLogInfo("Added a MMFader to the scene. You're all set.");
		}
	}
}
#endif