using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A feedback used to play the specified state on the target Animator, either in normalized or fixed time.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will allow you to play the specified state on the target Animator, either in normalized or fixed time.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Animation/Animator Play State")]
	public class MMF_AnimatorPlayState : MMF_Feedback 
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// the possible modes that pilot triggers        
		public enum TriggerModes { SetTrigger, ResetTrigger }
        
		/// the possible ways to set a value
		public enum ValueModes { None, Constant, Random, Incremental }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
		#endif
		
		/// the duration of this feedback is the declared duration 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();
		
		public enum Modes { NormalizedTime, FixedTime } 

		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("the animator whose parameters you want to update")]
		public Animator BoundAnimator;
		/// the list of extra animators whose parameters you want to update
		[Tooltip("the list of extra animators whose parameters you want to update")]
		public List<Animator> ExtraBoundAnimators;
		/// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.")]
		public float DeclaredDuration = 0f;
        
		[MMFInspectorGroup("State", true, 16)]
		/// The name of the state to play on the target animator
		[Tooltip("The name of the state to play on the target animator")]
		public string StateName;
		/// Whether to play the state at a normalized time (between 0 and 1) or a fixed time (in seconds)
		[Tooltip("Whether to play the state at a normalized time (between 0 and 1) or a fixed time (in seconds)")]
		public Modes Mode = Modes.NormalizedTime;
		/// The time offset between zero and one at which to play the specified state
		[Tooltip("The time offset between zero and one at which to play the specified state")]
		[MMFEnumCondition("Mode", (int)Modes.NormalizedTime)]
		public float NormalizedTime = 0f;
		/// The time offset (in seconds) at which to play the specified state
		[Tooltip("The time offset (in seconds) at which to play the specified state")]
		[MMFEnumCondition("Mode", (int)Modes.FixedTime)]
		public float FixedTime = 0f;
		/// The layer index. If layer is -1, it plays the first state with the given state name or hash.
		[Tooltip("The layer index. If layer is -1, it plays the first state with the given state name or hash.")]
		public int LayerIndex = -1;
		/// the name of the Animator layer you want the state to play on. This is optional. If left empty, the layer ID above will be used, if not empty, the Layer id specified above will be ignored.
		[Tooltip("the name of the Animator layer you want the state to play on. This is optional. If left empty, the layer ID above will be used, if not empty, the Layer id specified above will be ignored.")]
		public string LayerName = "";

		[MMFInspectorGroup("Layer Weights", true, 22)]
		/// whether or not to set layer weights on the specified layer when playing this feedback
		[Tooltip("whether or not to set layer weights on the specified layer when playing this feedback")]
		public bool SetLayerWeight = false;
		/// the index of the layer to target when changing layer weights
		[Tooltip("the index of the layer to target when changing layer weights")]
		[MMFCondition("SetLayerWeight", true)]
		public int TargetLayerIndex = 1;
		/// the new weight to set on the target animator layer
		[Tooltip("the new weight to set on the target animator layer")]
		[MMFCondition("SetLayerWeight", true)]
		public float NewWeight = 0.5f;

		protected int _targetParameter;
		protected int _layerID;

		/// <summary>
		/// Custom Init
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			_targetParameter = Animator.StringToHash(StateName);
			_layerID = TargetLayerIndex;
			if ((LayerName != "") && (BoundAnimator != null))
			{
				_layerID = BoundAnimator.GetLayerIndex(LayerName);
			}
		}

		/// <summary>
		/// On Play, checks if an animator is bound and plays the specified state
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("[Animator Play State Feedback] The animator play state feedback on "+Owner.name+" doesn't have a BoundAnimator, it won't work. You need to specify one in its inspector.");
				return;
			}

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			PlayState(BoundAnimator, intensityMultiplier);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				PlayState(animator, intensityMultiplier);
			}
		}

		/// <summary>
		/// Plays the specified state on the target animator
		/// </summary>
		/// <param name="targetAnimator"></param>
		/// <param name="intensityMultiplier"></param>
		protected virtual void PlayState(Animator targetAnimator, float intensityMultiplier)
		{
			if (SetLayerWeight)
			{
				targetAnimator.SetLayerWeight(_layerID, NewWeight);
			}
			
			if (Mode == Modes.NormalizedTime)
			{
				targetAnimator.Play(_targetParameter, LayerIndex, NormalizedTime);
			}
			else
			{
				targetAnimator.PlayInFixedTime(_targetParameter, LayerIndex, FixedTime);
			}
		}
        
		/// <summary>
		/// On stop, we do nothing
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
		}
	}
}