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
	/// This feedback will let you animate the scale of the target object over time, with a spring + squash and stretch effect
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Transform/Squash and Stretch Spring")]
	[FeedbackHelp("This feedback will let you animate the scale of the target object over time, with a spring + squash and stretch effect")]
	public class MMF_SquashAndStretchSpring : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateScaleTarget == null); }
		public override string RequiredTargetText { get { return AnimateScaleTarget != null ? AnimateScaleTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that an AnimateScaleTarget be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateScaleTarget = FindAutomatedTarget<Transform>();
		/// the duration of this feedback is the duration of the scale animation
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;

		public enum Modes { MoveTo, MoveToAdditive, Bump }
		public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }
		
		[MMFInspectorGroup("Target", true, 12, true)]
		/// the object to animate
		[Tooltip("the object to animate")]
		public Transform AnimateScaleTarget;
		/// spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player
		[Tooltip("spring duration is determined by the spring (and could be impacted real time), so it's up to you to determine how long this feedback should last, from the point of view of its parent MMF Player")]
		public float DeclaredDuration = 0f;
		/// the axis on which to operate squashing and stretching
		[Tooltip("the axis on which to operate squashing and stretching")]
		public PossibleAxis Axis = PossibleAxis.XtoYZ;
		
		[MMFInspectorGroup("Spring Settings", true, 18)]
		/// the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly
		[Tooltip("the dumping ratio determines how fast the spring will evolve after a disturbance. At a low value, it'll oscillate for a long time, while closer to 1 it'll stop oscillating quickly")]
		[Range(0.01f, 1f)]
		public float Damping = 0.4f;
		/// the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second
		[Tooltip("the frequency determines how fast the spring will oscillate when disturbed, low frequency means less oscillations per second, high frequency means more oscillations per second")]
		public float Frequency = 6f;
		
		[MMFInspectorGroup("Spring Mode", true, 19)]
		/// the chosen mode for this spring. MoveTo will move the target the specified scale (randomized between min and max). MoveToAdditive will add the specified scale (randomized between min and max) to the target's current scale. Bump will bump the target's scale by the specified power (randomized between min and max)
		[Tooltip("the chosen mode for this spring. MoveTo will move the target the specified scale (randomized between min and max). MoveToAdditive will add the specified scale (randomized between min and max) to the target's current scale. Bump will bump the target's scale by the specified power (randomized between min and max)")]
		public Modes Mode = Modes.Bump;
		/// the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("the min value from which to pick a random target value when in MoveTo or MoveToAdditive modes")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public float MoveToMin = 1f;
		/// the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes
		[Tooltip("the max value from which to pick a random target value when in MoveTo or MoveToAdditive modes")]
		[MMFEnumCondition("Mode", (int)Modes.MoveTo, (int)Modes.MoveToAdditive)]
		public float MoveToMax = 2f;

		/// the min value from which to pick a random bump amount when in Bump mode
		[Tooltip("the min value from which to pick a random bump amount when in Bump mode")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public float BumpScaleMin = 20f;

		/// the max value from which to pick a random bump amount when in Bump mode
		[Tooltip("the max value from which to pick a random bump amount when in Bump mode")]
		[MMFEnumCondition("Mode", (int)Modes.Bump)]
		public float BumpScaleMax = 30f;

		protected float _currentValue = 0f;
		protected float _targetValue = 0f;
		protected float _velocity = 0f;
		
		protected virtual bool LowVelocity => Mathf.Abs(_velocity) < _velocityLowThreshold;
		protected Coroutine _coroutine;
		protected float _velocityLowThreshold = 0.001f;
		
		protected Vector3 _newScale;
		protected Vector3 _initialScale;

		/// <summary>
		/// On init we store our initial scale
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateScaleTarget != null))
			{
				GetInitialValues();
			}
		}

		/// <summary>
		/// Stores initial scale for future use
		/// </summary>
		protected virtual void GetInitialValues()
		{
			_initialScale = AnimateScaleTarget.localScale;
			_currentValue = AnimateScaleTarget.localScale.x;
			_targetValue = _currentValue;
		}

		/// <summary>
		/// On Play, triggers the scale animation
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateScaleTarget == null))
			{
				return;
			}

			if (_coroutine != null)	{ Owner.StopCoroutine(_coroutine); }

			switch (Mode)
			{
				case Modes.MoveTo:
					_targetValue = Random.Range(MoveToMin, MoveToMax);
					break;
				case Modes.MoveToAdditive:
					_targetValue += Random.Range(MoveToMin, MoveToMax);
					break;
				case Modes.Bump:
					_velocity = Random.Range(BumpScaleMin, BumpScaleMax);
					break;
			}
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
			
			_velocity = 0f;
			_currentValue = _targetValue;
			ApplyValue();
			
			IsPlaying = false;
		}

		/// <summary>
		/// Updates the spring's values
		/// </summary>
		protected virtual void UpdateSpring()
		{
			MMMaths.Spring(ref _currentValue, _targetValue, ref _velocity, Damping, Frequency, FeedbackDeltaTime);
			ApplyValue();
		}

		/// <summary>
		/// Applies the current spring value to the target
		/// </summary>
		protected virtual void ApplyValue()
		{
			float newValue = _currentValue;
			float invertScale = 1 / Mathf.Sqrt(newValue);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
			}
			_newScale.x = Mathf.Abs(_newScale.x);
			_newScale.y = Mathf.Abs(_newScale.y);
			_newScale.z = Mathf.Abs(_newScale.z);
			AnimateScaleTarget.localScale = _newScale;
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
			_velocity = 0f;
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
			if (Active && FeedbackTypeAuthorized && (AnimateScaleTarget != null))
			{
				if (_coroutine != null)
				{
					Owner.StopCoroutine(_coroutine);
				}
				_currentValue = _targetValue;
				IsPlaying = false;
				_velocity = 0f;
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
			_currentValue = _initialScale.x;
			_targetValue = _currentValue;
			ApplyValue();
		}
	}
}