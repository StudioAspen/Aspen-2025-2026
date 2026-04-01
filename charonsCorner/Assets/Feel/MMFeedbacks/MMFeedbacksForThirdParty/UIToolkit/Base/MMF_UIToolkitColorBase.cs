using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A base feedback to set a color on a target UI Document
	/// </summary>
	[AddComponentMenu("")]
	public class MMF_UIToolkitColorBase : MMF_UIToolkit
	{
		/// the duration of this feedback is whatever value's been defined for it
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant }
		
		[MMFInspectorGroup("Color", true, 55, true)]
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("whether the feedback should affect the Image instantly or over a period of time")]
		public Modes Mode = Modes.OverTime;
		/// how long the Image should change over time
		[Tooltip("how long the Image should change over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over")] 
		public bool AllowAdditivePlays = false;
		/// whether or not to modify the color of the image
		[Tooltip("whether or not to modify the color of the image")]
		public bool ModifyColor = true;
		/// the colors to apply to the Image over time
		[Tooltip("the colors to apply to the Image over time")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime = 
			new Gradient()
			{
				colorKeys = new GradientColorKey[]
				{
					new GradientColorKey(Color.white, 0f),
					new GradientColorKey(Color.red, 0.5f),
					new GradientColorKey(Color.white, 1f)
				},
				alphaKeys = new GradientAlphaKey[]
				{
					new GradientAlphaKey(1f, 0f),
					new GradientAlphaKey(1f, 0.5f),
					new GradientAlphaKey(1f, 1f)
				}
			};
		/// the color to move to in instant mode
		[Tooltip("the color to move to in instant mode")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;
		/// if this is true, the initial color will be applied to the gradient start
		[Tooltip("if this is true, the initial color will be applied to the gradient start")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public bool ApplyInitialColorToGradientStart = false;
		/// if this is true, the initial color will be applied to the gradient end
		[Tooltip("if this is true, the initial color will be applied to the gradient end")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public bool ApplyInitialColorToGradientEnd = false;
		/// if this is true, the initial color will be applied to the gradient start and end on play
		[FormerlySerializedAs("GrabInitialColorsOnPlay")]
		[Tooltip("if this is true, the initial color will be applied to the gradient start and end on play")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public bool ApplyInitialColorsOnPlay = true;

		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected Color _initialInstantColor;

		/// <summary>
		/// On init we turn the Image off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			HandleApplyInitialColors();
        
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
			
			_initialInstantColor = GetInitialColor();
		}

		protected virtual void HandleApplyInitialColors()
		{
			var colorKeys = ColorOverTime.colorKeys;
			var alphaKeys = ColorOverTime.alphaKeys;
			
			if (ApplyInitialColorToGradientStart)
			{
				colorKeys[0] = new GradientColorKey(GetInitialColor(),0f);
				alphaKeys[0] = new GradientAlphaKey(GetInitialColor().a,0f);
			}

			if (ApplyInitialColorToGradientEnd)
			{
				int lastIndex = ColorOverTime.colorKeys.Length - 1; 
				colorKeys[lastIndex] = new GradientColorKey(GetInitialColor(),1f);
				alphaKeys[lastIndex] = new GradientAlphaKey(GetInitialColor().a,1f);
			}
			
			if (ApplyInitialColorToGradientEnd || ApplyInitialColorToGradientStart)
			{
				ColorOverTime.SetKeys(colorKeys, alphaKeys);
			}
		}

		protected virtual void ApplyColor(Color newColor)
		{
			
		}

		protected virtual Color GetInitialColor()
		{
			return Color.white;
		}

		/// <summary>
		/// On Play we turn our Image on and start an over time coroutine if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if ((_visualElements == null) || (_visualElements.Count == 0))
			{
				return;
			}
        
			_initialColor = GetInitialColor();

			if (ApplyInitialColorsOnPlay)
			{
				HandleApplyInitialColors();
			}
			
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						if (NormalPlayDirection)
						{
							ApplyColor(InstantColor);
						}
						else
						{
							ApplyColor(_initialInstantColor);
						}
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
			}
		}

		/// <summary>
		/// This coroutine will modify the values on the Image
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ImageSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetImageValues(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetImageValues(FinalNormalizedTime);
			
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

		/// <summary>
		/// Sets the various values on the sprite renderer on a specified time (between 0 and 1)
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetImageValues(float time)
		{
			if (ModifyColor)
			{
				ApplyColor(ColorOverTime.Evaluate(time));
			}
		}

		/// <summary>
		/// Turns the sprite renderer off on stop
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
			base.CustomStopFeedback(position, feedbacksIntensity);
			_coroutine = null;
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
			ApplyColor(_initialColor);
		}
	}
}