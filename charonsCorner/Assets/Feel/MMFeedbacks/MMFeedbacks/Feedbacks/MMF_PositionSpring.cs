using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position of the target object over time, with a spring effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Position Spring")]
	[FeedbackHelp("This feedback will let you animate the position of the target object over time, with a spring effect.")]
	public class MMF_PositionSpring : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimatePositionTarget == null); }
		public override string RequiredTargetText { get { return AnimatePositionTarget != null ? AnimatePositionTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that an AnimatePositionTarget be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimatePositionTarget = FindAutomatedTarget<Transform>();
		/// the duration of this feedback is the duration of the position animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		public enum Spaces { World, Local, RectTransform }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform AnimatePositionTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player")]
		public float DeclaredDuration = 0f;
		/// the space in which to move the position in
		[Tooltip("the space in which to move the position in")]
		public Spaces Space = Spaces.World;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly")]
		[Range(0.01f, 1f)]
		public float DampingX = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second")]
		public float FrequencyX = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly")]
		[Range(0.01f, 1f)]
		public float DampingY = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second")]
		public float FrequencyY = 6f;
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly")]
		[Range(0.01f, 1f)]
		public float DampingZ = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second")]
		public float FrequencyZ = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified position (randomized between min and max). MoveToAdditive will add the specified position (randomized between min and max) to the target's current position. Bump will bump the target's position by the specified power (randomized between min and max)
		[Tooltip("the chosen mode for this spring. MoveTo will move the target the specified position (randomized between min and max). MoveToAdditive will add the specified position (randomized between min and max) to the target's current position. Bump will bump the target's position by the specified power (randomized between min and max)")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToPositionMin = new Vector3(1f, 1f, 1f);
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Vector3 MoveToPositionMax = new Vector3(2f, 2f, 2f);
		
		/// an optional transform you can use to determine the target position. If this is set, the MoveToPositionMin and MoveToPositionMax values will be used as offsets from this transform's position in Additive mode
		[Tooltip("an optional transform you can use to determine the target position. If this is set, the MoveToPositionMin and MoveToPositionMax values will be used as offsets from this transform's position in Additive mode")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public Transform MoveToTransform;
		
		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("the min value from which to pick a random bump amount when in Bump mode")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpPositionMin = new Vector3(0f, 20f, 0f);
		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("the max value from which to pick a random bump amount when in Bump mode")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public Vector3 BumpPositionMax = new Vector3(0f, 30f, 0f);

		public bool ForceAbsolute = false;
        
		protected Vector3 _currentValue = Vector3.zero;
		protected Vector3 _targetValue = Vector3.zero;
		protected Vector3 _velocity = Vector3.zero;
		protected Vector3 _initialTargetValue = Vector3.zero;
		
		protected Vector3 _initialPosition;
		protected virtual bool LowVelocity => (Mathf.Abs(_velocity.x) + Mathf.Abs(_velocity.y) + Mathf.Abs(_velocity.z)) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;
		protected RectTransform _rectTransform;

		/// <summary>
		/// On init we store our initial position
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Space == Spaces.RectTransform)
			{
				_rectTransform = AnimatePositionTarget.GetComponent<RectTransform>();
			}
			if (Active && (AnimatePositionTarget != null))
			{
				GetInitialValues();
			}
		}

		/// <summary>
		/// Stores initial position for future use
		/// </summary>
		protected virtual void GetInitialValues()
		{
			switch (Space)
			{
				case Spaces.World:
					_initialPosition = AnimatePositionTarget.position;
					break;
				case Spaces.Local:
					_initialPosition = AnimatePositionTarget.localPosition;
					break;
				case Spaces.RectTransform:
					_initialPosition = _rectTransform.anchoredPosition3D;
					break;
			}
			_currentValue = _initialPosition;
			_targetValue = _currentValue;
		}

		/// <summary>
		/// On Play, triggers the position animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimatePositionTarget == null))
			{
				return;
			}

			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue.x = Random.Range(MoveToPositionMin.x, MoveToPositionMax.x);
					_targetValue.y = Random.Range(MoveToPositionMin.y, MoveToPositionMax.y);
					_targetValue.z = Random.Range(MoveToPositionMin.z, MoveToPositionMax.z);
					break;
				case Modes.MoveToAdditive:
					_targetValue.x += Random.Range(MoveToPositionMin.x, MoveToPositionMax.x);
					_targetValue.y += Random.Range(MoveToPositionMin.y, MoveToPositionMax.y);
					_targetValue.z += Random.Range(MoveToPositionMin.z, MoveToPositionMax.z);
					break;
				case Modes.Bump:
					_velocity.x = Random.Range(BumpPositionMin.x, BumpPositionMax.x);
					_velocity.y = Random.Range(BumpPositionMin.y, BumpPositionMax.y);
					_velocity.z = Random.Range(BumpPositionMin.z, BumpPositionMax.z);
					float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
					_velocity.x *= intensityMultiplier;
					break;
			}

			_initialTargetValue = _targetValue;
			_coroutine = Owner.StartCoroutine(Spring());
		}

		/// <summary>
		/// a coroutine running on the Owner used to move the spring
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator Spring()
		{
			IsPlaying = true;
			UpdateSpring();
			while (!LowVelocity)
			{
				yield return null;
				UpdateSpring();
				ApplyValue();
			}
			
			_velocity.x = 0f;
			_velocity.y = 0f;
			_velocity.z = 0f;
			_currentValue = _targetValue;
			ApplyValue();
			
			IsPlaying = false;
		}

		/// <summary>
		/// Updates the spring's values
		/// </summary>
		protected virtual void UpdateSpring()
		{
			if (MoveToTransform != null)
			{
				if (Mode == Modes.MoveTo)
				{
					_targetValue = MoveToTransform.position;
				}
				else if (Mode == Modes.MoveToAdditive)
				{
					_targetValue = MoveToTransform.position + _initialTargetValue;
				}
			}
			
			MMMaths.Spring(ref _currentValue.x, _targetValue.x, ref _velocity.x, DampingX, FrequencyX, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.y, _targetValue.y, ref _velocity.y, DampingY, FrequencyY, FeedbackDeltaTime);
			MMMaths.Spring(ref _currentValue.z, _targetValue.z, ref _velocity.z, DampingZ, FrequencyZ, FeedbackDeltaTime);
			ApplyValue();
		}

		protected Vector3 _appliedPosition;
		
		/// <summary>
		/// Applies the current spring value to the target
		/// </summary>
		protected virtual void ApplyValue()
		{
			_appliedPosition = _currentValue;
			if (ForceAbsolute)
			{
				_appliedPosition.x = Mathf.Abs(_appliedPosition.x - _initialPosition.x) + _initialPosition.x;
				_appliedPosition.y = Mathf.Abs(_appliedPosition.y - _initialPosition.y) + _initialPosition.y;
				_appliedPosition.z = Mathf.Abs(_appliedPosition.z - _initialPosition.z) + _initialPosition.z;
			}
			
			if (Space == Spaces.World)
			{
				AnimatePositionTarget.position = _appliedPosition; 	
			}
			else if (Space == Spaces.RectTransform)
			{
				_rectTransform.anchoredPosition3D = _appliedPosition;
			}
			else if (Space == Spaces.Local)
			{
				AnimatePositionTarget.localPosition = _appliedPosition;
			}
		}

		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
			}
			IsPlaying = false;
			_velocity.x = 0f;
			_velocity.y = 0f;
			_velocity.z = 0f;
			_targetValue = _currentValue;
			ApplyValue();
		}
		
		/// <summary>
		/// Skips to the end, matching the target value
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (AnimatePositionTarget != null))
			{
				if (_coroutine != null)
				{
					Owner.StopCoroutine(_coroutine);
				}
				_currentValue = _targetValue;
				IsPlaying = false;
				_velocity.x = 0f;
				_velocity.y = 0f;
				_velocity.z = 0f;
				ApplyValue();
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
			_currentValue = _initialPosition;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}