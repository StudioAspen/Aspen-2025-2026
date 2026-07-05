using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("This feedback will let you animate the position/rotation/scale of a target transform to match the one of a destination transform.")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[System.Serializable]
	[FeedbackPath("Transform/Destination")]
	public class MMF_DestinationTransform : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the possible timescales this feedback can animate on
		public enum TimeScales { Scaled, Unscaled }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() 
		{
			if (UseMultiTargets)
			{
				return (MultiTargetTransforms == null) || (MultiTargetTransforms.Count == 0) || (MultiDestinations == null) || (MultiDestinations.Count == 0);
			}
			return (TargetTransform == null) || (Destination == null); 
		}
		public override string RequiredTargetText 
		{ 
			get 
			{
				if (UseMultiTargets)
				{
					return MultiTargetTransforms != null ? MultiTargetTransforms.Count + " targets" : "0 targets";
				}
				return TargetTransform != null ? TargetTransform.name : "";  
			} 
		}
		public override string RequiresSetupText 
		{ 
			get 
			{
				if (UseMultiTargets)
				{
					return "This feedback requires that MultiTargetTransforms and MultiDestinations be set to be able to work properly. You can set them below.";
				}
				return "This feedback requires that a TargetTransform and a Destination be set to be able to work properly. You can set one below."; 
			} 
		}
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetTransform = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Target to animate", true, 61, true)]
		/// the target transform we want to animate properties on
		[Tooltip("the target transform we want to animate properties on")]
		[MMFCondition("UseMultiTargets", false)]
		public Transform TargetTransform;

		/// whether or not to use the multi-target lists below
		[Tooltip("whether or not to use the multi-target lists below")]
		public bool UseMultiTargets = false;
		/// the list of transforms to animate
		[Tooltip("the list of transforms to animate")]
		[MMFCondition("UseMultiTargets", true)]
		public List<Transform> MultiTargetTransforms;
		/// the list of destination transforms whose properties we want to match
		[Tooltip("the list of destination transforms whose properties we want to match")]
		[MMFCondition("UseMultiTargets", true)]
		public List<Transform> MultiDestinations;
        
		/// whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead
		[Tooltip("whether or not we want to force an origin transform. If not, the current position of the target transform will be used as origin instead")]
		public bool ForceOrigin = false;
		/// the transform to use as origin in ForceOrigin mode
		[Tooltip("the transform to use as origin in ForceOrigin mode")]
		[MMFCondition("ForceOrigin", true)] 
		public Transform Origin;
		/// the destination transform whose properties we want to match 
		[Tooltip("the destination transform whose properties we want to match")]
		[MMFCondition("UseMultiTargets", false)]
		public Transform Destination;
        
		[MMFInspectorGroup("Transition", true, 63)]
		/// a global curve to animate all properties on, unless dedicated ones are specified
		[Tooltip("a global curve to animate all properties on, unless dedicated ones are specified")]
		public MMTweenType GlobalAnimationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the duration of the transition, in seconds
		[Tooltip("the duration of the transition, in seconds")]
		public float Duration = 0.2f;
		/// if this is true, the destination will be updated every frame, allowing for dynamic changes to the destination transform, otherwise the destination will be cached on init and not updated after that
		[Tooltip("if this is true, the destination will be updated every frame, allowing for dynamic changes to the destination transform, otherwise the destination will be cached on init and not updated after that")]
		public bool UpdateDestinationEveryFrame = false;

		[MMFInspectorGroup("Axis Locks", true, 64)]
        
		/// whether or not to animate the X position
		[Tooltip("whether or not to animate the X Position")]
		public bool AnimatePositionX = true;
		/// whether or not to animate the Y position
		[Tooltip("whether or not to animate the Y Position")]
		public bool AnimatePositionY = true;
		/// whether or not to animate the Z position
		[Tooltip("whether or not to animate the Z Position")]
		public bool AnimatePositionZ = true;
		/// whether or not to animate the X rotation
		[Tooltip("whether or not to animate the X rotation")]
		public bool AnimateRotationX = true;
		/// whether or not to animate the Y rotation
		[Tooltip("whether or not to animate the Y rotation")]
		public bool AnimateRotationY = true;
		/// whether or not to animate the Z rotation
		[Tooltip("whether or not to animate the Z rotation")]
		public bool AnimateRotationZ = true;
		/// whether or not to animate the W rotation
		[Tooltip("whether or not to animate the W rotation")]
		public bool AnimateRotationW = true;
		/// whether or not to animate the X scale
		[Tooltip("whether or not to animate the X scale")]
		public bool AnimateScaleX = true;
		/// whether or not to animate the Y scale
		[Tooltip("whether or not to animate the Y scale")]
		public bool AnimateScaleY = true;
		/// whether or not to animate the Z scale
		[Tooltip("whether or not to animate the Z scale")]
		public bool AnimateScaleZ = true;

		[MMFInspectorGroup("Separate Curves", true, 65)]
		/// whether or not to use a separate animation curve to animate the position
		[Tooltip("whether or not to use a separate animation curve to animate the position")]
		public bool SeparatePositionCurve = false;
		/// the curve to use to animate the position on
		[Tooltip("the curve to use to animate the position on")]
		public MMTweenType AnimatePositionTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)), "SeparatePositionCurve");
        
		/// whether or not to use a separate animation curve to animate the rotation
		[Tooltip("whether or not to use a separate animation curve to animate the rotation")]
		public bool SeparateRotationCurve = false;
		/// the curve to use to animate the rotation on
		[Tooltip("the curve to use to animate the rotation on")]
		public MMTweenType AnimateRotationTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)), "SeparateRotationCurve");
        
		/// whether or not to use a separate animation curve to animate the scale
		[Tooltip("whether or not to use a separate animation curve to animate the scale")]
		public bool SeparateScaleCurve = false;
		/// the curve to use to animate the scale on
		[Tooltip("the curve to use to animate the scale on")] 
		public MMTweenType AnimateScaleTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)), "SeparateScaleCurve");
        
		/// the duration of this feedback is the duration of the movement
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		/// a global curve to animate all properties on, unless dedicated ones are specified
		[HideInInspector] public AnimationCurve GlobalAnimationCurve = null;
		/// the curve to use to animate the position on
		[HideInInspector] public AnimationCurve AnimateScaleCurve = null;
		/// the curve to use to animate the rotation on
		[HideInInspector] public AnimationCurve AnimatePositionCurve = null;
		/// the curve to use to animate the scale on
		[HideInInspector] public AnimationCurve AnimateRotationCurve = null;
		
		protected Coroutine _coroutine;
		protected Vector3 _newPosition;
		protected Quaternion _newRotation;
		protected Vector3 _newScale;
		protected Vector3 _pointAPosition;
		protected Vector3 _pointBPosition;
		protected Quaternion _pointARotation;
		protected Quaternion _pointBRotation;
		protected Vector3 _pointAScale;
		protected Vector3 _pointBScale;
		protected MMTweenType _animationTweenType;

		protected Vector3 _initialPosition;
		protected Vector3 _initialScale;
		protected Quaternion _initialRotation;

		protected List<Vector3> _initialPositions;
		protected List<Vector3> _initialScales;
		protected List<Quaternion> _initialRotations;
		protected List<Vector3> _pointAPositions;
		protected List<Vector3> _pointBPositions;
		protected List<Quaternion> _pointARotations;
		protected List<Quaternion> _pointBRotations;
		protected List<Vector3> _pointAScales;
		protected List<Vector3> _pointBScales;
        
		/// <summary>
		/// On Play we animate the pos/rotation/scale of the target transform towards its destination
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (UseMultiTargets)
			{
				if (MultiTargetTransforms == null || MultiTargetTransforms.Count == 0)
				{
					return;
				}
			}
			else
			{
				if (TargetTransform == null)
				{
					return;
				}
			}
			
			if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
			_coroutine = Owner.StartCoroutine(AnimateToDestination());
		}

		/// <summary>
		/// A coroutine used to animate the pos/rotation/scale of the target transform towards its destination
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator AnimateToDestination()
		{
			if (UseMultiTargets)
			{
				PrepareMultiTargets();
			}
			else
			{
				_initialPosition = TargetTransform.position;
				_initialRotation = TargetTransform.rotation;
				_initialScale = TargetTransform.localScale;

				_pointAPosition = ForceOrigin ? Origin.transform.position : TargetTransform.position;
				_pointARotation = ForceOrigin ? Origin.transform.rotation : TargetTransform.rotation;
				_pointAScale = ForceOrigin ? Origin.transform.localScale : TargetTransform.localScale;
			}
			
			CacheDestinationValues();

			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				if (UpdateDestinationEveryFrame)
				{
					CacheDestinationValues();
				}
				float percent = Mathf.Clamp01(journey / FeedbackDuration);
				ChangeTransformValues(percent);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			// set final position
			ChangeTransformValues(1f);
			
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

		protected virtual void PrepareMultiTargets()
		{
			if (_initialPositions == null) { _initialPositions = new List<Vector3>(); }
			if (_initialRotations == null) { _initialRotations = new List<Quaternion>(); }
			if (_initialScales == null) { _initialScales = new List<Vector3>(); }
			if (_pointAPositions == null) { _pointAPositions = new List<Vector3>(); }
			if (_pointARotations == null) { _pointARotations = new List<Quaternion>(); }
			if (_pointAScales == null) { _pointAScales = new List<Vector3>(); }
			
			_initialPositions.Clear();
			_initialRotations.Clear();
			_initialScales.Clear();
			_pointAPositions.Clear();
			_pointARotations.Clear();
			_pointAScales.Clear();

			for (int i = 0; i < MultiTargetTransforms.Count; i++)
			{
				if (MultiTargetTransforms[i] == null)
				{
					_initialPositions.Add(Vector3.zero);
					_initialRotations.Add(Quaternion.identity);
					_initialScales.Add(Vector3.one);
					_pointAPositions.Add(Vector3.zero);
					_pointARotations.Add(Quaternion.identity);
					_pointAScales.Add(Vector3.one);
					continue;
				}

				_initialPositions.Add(MultiTargetTransforms[i].position);
				_initialRotations.Add(MultiTargetTransforms[i].rotation);
				_initialScales.Add(MultiTargetTransforms[i].localScale);

				_pointAPositions.Add(ForceOrigin ? Origin.transform.position : MultiTargetTransforms[i].position);
				_pointARotations.Add(ForceOrigin ? Origin.transform.rotation : MultiTargetTransforms[i].rotation);
				_pointAScales.Add(ForceOrigin ? Origin.transform.localScale : MultiTargetTransforms[i].localScale);
			}
		}

		protected virtual void CacheDestinationValues()
		{
			if (UseMultiTargets)
			{
				CacheMultiDestinationValues();
				return;
			}

			_pointBPosition = Destination.transform.position;

			if (!AnimatePositionX) { _pointAPosition.x = TargetTransform.position.x; _pointBPosition.x = _pointAPosition.x; }
			if (!AnimatePositionY) { _pointAPosition.y = TargetTransform.position.y; _pointBPosition.y = _pointAPosition.y; }
			if (!AnimatePositionZ) { _pointAPosition.z = TargetTransform.position.z; _pointBPosition.z = _pointAPosition.z; }
            
			_pointBRotation = Destination.transform.rotation;
            
			if (!AnimateRotationX) { _pointARotation.x = TargetTransform.rotation.x; _pointBRotation.x = _pointARotation.x; }
			if (!AnimateRotationY) { _pointARotation.y = TargetTransform.rotation.y; _pointBRotation.y = _pointARotation.y; }
			if (!AnimateRotationZ) { _pointARotation.z = TargetTransform.rotation.z; _pointBRotation.z = _pointARotation.z; }
			if (!AnimateRotationW) { _pointARotation.w = TargetTransform.rotation.w; _pointBRotation.w = _pointARotation.w; }

			_pointBScale = Destination.transform.localScale;
            
			if (!AnimateScaleX) { _pointAScale.x = TargetTransform.localScale.x; _pointBScale.x = _pointAScale.x; }
			if (!AnimateScaleY) { _pointAScale.y = TargetTransform.localScale.y; _pointBScale.y = _pointAScale.y; }
			if (!AnimateScaleZ) { _pointAScale.z = TargetTransform.localScale.z; _pointBScale.z = _pointAScale.z; }
		}

		protected virtual void CacheMultiDestinationValues()
		{
			if (_pointBPositions == null) { _pointBPositions = new List<Vector3>(); }
			if (_pointBRotations == null) { _pointBRotations = new List<Quaternion>(); }
			if (_pointBScales == null) { _pointBScales = new List<Vector3>(); }

			_pointBPositions.Clear();
			_pointBRotations.Clear();
			_pointBScales.Clear();

			for (int i = 0; i < MultiTargetTransforms.Count; i++)
			{
				if (MultiTargetTransforms[i] == null)
				{
					_pointBPositions.Add(Vector3.zero);
					_pointBRotations.Add(Quaternion.identity);
					_pointBScales.Add(Vector3.one);
					continue;
				}

				Transform destination = GetDestination(i);
				if (destination == null)
				{
					_pointBPositions.Add(MultiTargetTransforms[i].position);
					_pointBRotations.Add(MultiTargetTransforms[i].rotation);
					_pointBScales.Add(MultiTargetTransforms[i].localScale);
					continue;
				}

				Vector3 pointBPosition = destination.position;
				Vector3 pointAPosition = _pointAPositions[i];
				if (!AnimatePositionX) { pointAPosition.x = MultiTargetTransforms[i].position.x; pointBPosition.x = pointAPosition.x; }
				if (!AnimatePositionY) { pointAPosition.y = MultiTargetTransforms[i].position.y; pointBPosition.y = pointAPosition.y; }
				if (!AnimatePositionZ) { pointAPosition.z = MultiTargetTransforms[i].position.z; pointBPosition.z = pointAPosition.z; }
				_pointAPositions[i] = pointAPosition;
				_pointBPositions.Add(pointBPosition);

				Quaternion pointBRotation = destination.rotation;
				Quaternion pointARotation = _pointARotations[i];
				if (!AnimateRotationX) { pointARotation.x = MultiTargetTransforms[i].rotation.x; pointBRotation.x = pointARotation.x; }
				if (!AnimateRotationY) { pointARotation.y = MultiTargetTransforms[i].rotation.y; pointBRotation.y = pointARotation.y; }
				if (!AnimateRotationZ) { pointARotation.z = MultiTargetTransforms[i].rotation.z; pointBRotation.z = pointARotation.z; }
				if (!AnimateRotationW) { pointARotation.w = MultiTargetTransforms[i].rotation.w; pointBRotation.w = pointARotation.w; }
				_pointARotations[i] = pointARotation;
				_pointBRotations.Add(pointBRotation);

				Vector3 pointBScale = destination.localScale;
				Vector3 pointAScale = _pointAScales[i];
				if (!AnimateScaleX) { pointAScale.x = MultiTargetTransforms[i].localScale.x; pointBScale.x = pointAScale.x; }
				if (!AnimateScaleY) { pointAScale.y = MultiTargetTransforms[i].localScale.y; pointBScale.y = pointAScale.y; }
				if (!AnimateScaleZ) { pointAScale.z = MultiTargetTransforms[i].localScale.z; pointBScale.z = pointAScale.z; }
				_pointAScales[i] = pointAScale;
				_pointBScales.Add(pointBScale);
			}
		}

		protected virtual Transform GetDestination(int index)
		{
			if (MultiDestinations == null || MultiDestinations.Count == 0)
			{
				return null;
			}
			if (index >= MultiDestinations.Count)
			{
				return MultiDestinations[MultiDestinations.Count - 1];
			}
			return MultiDestinations[index];
		}

		/// <summary>
		/// Computes the new position, rotation and scale for our transform, and applies it to the transform
		/// </summary>
		/// <param name="percent"></param>
		protected virtual void ChangeTransformValues(float percent)
		{
			if (UseMultiTargets)
			{
				for (int i = 0; i < MultiTargetTransforms.Count; i++)
				{
					if (MultiTargetTransforms[i] == null) { continue; }

					_animationTweenType = SeparatePositionCurve ? AnimatePositionTween : GlobalAnimationTween;
					_newPosition = Vector3.LerpUnclamped(_pointAPositions[i], _pointBPositions[i], _animationTweenType.Evaluate(percent));
                
					_animationTweenType = SeparateRotationCurve ? AnimateRotationTween : GlobalAnimationTween;
					_newRotation = Quaternion.LerpUnclamped(_pointARotations[i], _pointBRotations[i], _animationTweenType.Evaluate(percent));
                
					_animationTweenType = SeparateScaleCurve ? AnimateScaleTween : GlobalAnimationTween;
					_newScale = Vector3.LerpUnclamped(_pointAScales[i], _pointBScales[i], _animationTweenType.Evaluate(percent));
			
					MultiTargetTransforms[i].position = _newPosition;
					MultiTargetTransforms[i].rotation = _newRotation;
					MultiTargetTransforms[i].localScale = _newScale;
				}
			}
			else
			{
				_animationTweenType = SeparatePositionCurve ? AnimatePositionTween : GlobalAnimationTween;
				_newPosition = Vector3.LerpUnclamped(_pointAPosition, _pointBPosition, _animationTweenType.Evaluate(percent));
                
				_animationTweenType = SeparateRotationCurve ? AnimateRotationTween : GlobalAnimationTween;
				_newRotation = Quaternion.LerpUnclamped(_pointARotation, _pointBRotation, _animationTweenType.Evaluate(percent));
                
				_animationTweenType = SeparateScaleCurve ? AnimateScaleTween : GlobalAnimationTween;
				_newScale = Vector3.LerpUnclamped(_pointAScale, _pointBScale, _animationTweenType.Evaluate(percent));
			
				TargetTransform.position = _newPosition;
				TargetTransform.rotation = _newRotation;
				TargetTransform.localScale = _newScale;
			}
		}

		/// <summary>
		/// On Stop we stop our coroutine if needed
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
			IsPlaying = false;
            
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
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

			if (UseMultiTargets)
			{
				if (MultiTargetTransforms == null || _initialPositions == null || MultiTargetTransforms.Count != _initialPositions.Count)
				{
					return;
				}

				for (int i = 0; i < MultiTargetTransforms.Count; i++)
				{
					if (MultiTargetTransforms[i] != null)
					{
						MultiTargetTransforms[i].position = _initialPositions[i];
						MultiTargetTransforms[i].rotation = _initialRotations[i];
						MultiTargetTransforms[i].localScale = _initialScales[i];
					}
				}
			}
			else
			{
				if (TargetTransform != null)
				{
					TargetTransform.position = _initialPosition;
					TargetTransform.rotation = _initialRotation;
					TargetTransform.localScale = _initialScale;
				}
			}
		}
		
		/// <summary>
		/// On Validate, we migrate our deprecated animation curves to our tween types if needed
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(GlobalAnimationCurve, GlobalAnimationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimatePositionCurve, AnimatePositionTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationCurve, AnimateRotationTween, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateScaleCurve, AnimateScaleTween, Owner);
			if (string.IsNullOrEmpty(AnimatePositionTween.ConditionPropertyName))
			{
				AnimatePositionTween.ConditionPropertyName = "SeparatePositionCurve";
				AnimateRotationTween.ConditionPropertyName = "SeparateRotationCurve";
				AnimateScaleTween.ConditionPropertyName = "SeparateScaleCurve";
			}
		}
	}    
}