using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the color of a target sprite renderer over time, and flip it on X or Y. You can also use it to command one or many MMSpriteRendererShakers.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you change the alpha of a target sprite renderer over time.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/SpriteRenderer Alpha")]
	public class MMF_SpriteRendererAlpha : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundSpriteRenderer == null);
		public override string RequiredTargetText => BoundSpriteRenderer != null ? BoundSpriteRenderer.name : "";
		public override string RequiresSetupText => "This feedback requires that a BoundSpriteRenderer be set to be able to work properly. You can set one below.";
		#endif

		/// the duration of this feedback is the duration of the sprite renderer, or 0 if instant
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundSpriteRenderer = FindAutomatedTarget<SpriteRenderer>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ToDestinationAlpha, ToDestinationAlphaAndBack }
		/// the different ways to grab initial color
		public enum InitialAlphaModes { InitialAlphaOnInit, InitialAlphaOnPlay }

		[MMFInspectorGroup("Sprite Renderer", true, 51, true)]
		/// the SpriteRenderer to affect when playing the feedback
		[Tooltip("the SpriteRenderer to affect when playing the feedback")]
		public SpriteRenderer BoundSpriteRenderer;
		/// whether the feedback should affect the sprite renderer instantly or over a period of time
		[Tooltip("whether the feedback should affect the sprite renderer instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("how long the sprite renderer should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ToDestinationAlpha, (int)Modes.ToDestinationAlphaAndBack)]
		public float Duration = 0.2f;
		/// whether or not that sprite renderer should be turned off on start
		[Tooltip("whether or not that sprite renderer should be turned off on start")]
		public bool StartsOff = false;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// whether to grab the initial color to (potentially) go back to at init or when the feedback plays
		[Tooltip("whether to grab the initial color to (potentially) go back to at init or when the feedback plays")] 
		public InitialAlphaModes InitialAlphaMode = InitialAlphaModes.InitialAlphaOnPlay;
        
		/// the alpha to apply to the sprite renderer over time
		[Tooltip("the alpha to apply to the sprite renderer over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve AlphaOverTime = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
		/// the alpha to move to in instant mode
		[Tooltip("the alpha to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantAlpha;
		/// the alpha to move to in ToDestinationAlpha mode
		[Tooltip("the alpha to move to in ToDestinationAlpha mode")]
		[MMFEnumCondition("Mode",  (int)Modes.ToDestinationAlpha, (int)Modes.ToDestinationAlphaAndBack)]
		public float ToDestinationAlpha = 0.5f;
		/// the curve on which to tween in ToDestinationAlpha modes
		[Tooltip("the curve on which to tween in ToDestinationAlpha modes")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestinationAlpha, (int)Modes.ToDestinationAlphaAndBack)]
		public AnimationCurve ToDestinationAlphaCurve = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 1f));
        

		protected Coroutine _coroutine;
		protected float _initialAlpha;
        
		/// <summary>
		/// On init we turn the sprite renderer off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active)
			{
				if (StartsOff)
				{
					Turn(false);
				}
			}

			if ((BoundSpriteRenderer != null) && (InitialAlphaMode == InitialAlphaModes.InitialAlphaOnInit))
			{
				_initialAlpha = BoundSpriteRenderer.color.a;
			}
		}

		/// <summary>
		/// On Play we turn our sprite renderer on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (BoundSpriteRenderer == null)
			{
				Debug.LogWarning("[Sprite Renderer Feedback] The sprite renderer feedback on "+Owner.name+" doesn't have a BoundSpriteRenderer, it won't work. You need to specify one in its inspector.");
				return;
			}
            
			if (InitialAlphaMode == InitialAlphaModes.InitialAlphaOnPlay)
			{
				_initialAlpha = BoundSpriteRenderer.color.a;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					float newAlpha = NormalPlayDirection ? InstantAlpha : _initialAlpha;
					SetAlpha(newAlpha);
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererSequence());
					break;
				case Modes.ToDestinationAlpha:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(false));
					break;
				case Modes.ToDestinationAlphaAndBack:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(SpriteRendererToDestinationSequence(true));
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetSpriteRendererValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetSpriteRendererValues(FinalNormalizedTime);
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// This coroutine will modify the values on the SpriteRenderer
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator SpriteRendererToDestinationSequence(bool andBack)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				if (andBack)
				{
					remappedTime = (remappedTime < 0.5f)
						? MMFeedbacksHelpers.Remap(remappedTime, 0f, 0.5f, 0f, 1f)
						: MMFeedbacksHelpers.Remap(remappedTime, 0.5f, 1f, 1f, 0f);
				}
                
				float curveValue = ToDestinationAlphaCurve.Evaluate(remappedTime);
				float newAlpha = MMMaths.Remap(curveValue, 0f, 1f, _initialAlpha, ToDestinationAlpha);
				SetAlpha(newAlpha);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			float finalAlpha = andBack ? _initialAlpha : ToDestinationAlpha;
			SetAlpha(finalAlpha);
			
			if (StartsOff)
			{
				Turn(false);
			}            
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetSpriteRendererValues(float time)
		{
			float newAlpha = AlphaOverTime.Evaluate(time);
			SetAlpha(newAlpha);
		}

		/// <summary>
		/// Stops the transition on stop if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			Owner.StopCoroutine(_coroutine);
			IsPlaying = false;
			_coroutine = null;
		}

		/// <summary>
		/// Turns the sprite renderer on or off
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundSpriteRenderer.gameObject.SetActive(status);
			BoundSpriteRenderer.enabled = status;
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
			
			if (BoundSpriteRenderer != null)
			{
				SetAlpha(_initialAlpha);	
			}
		}

		protected virtual void SetAlpha(float newAlpha)
		{
			BoundSpriteRenderer.color = BoundSpriteRenderer.color.MMAlpha(newAlpha);
		}
        
		/// <summary>
		/// On disable, 
		/// </summary>
		public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}