using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Add this component to an object and it'll get moved towards the target at update, with or without interpolation based on your settings
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/Movement/MM Follow Target")]
	public class MMFollowTarget : MonoBehaviour
	{
		/// the possible update modes
		public enum UpdateModes { Update, FixedUpdate, LateUpdate }
		/// the possible follow modes
		public enum FollowModes { RegularLerp, MMLerp, MMSpring }
		/// whether to operate in world or local space
		public enum PositionSpaces { World, Local }

		[Header("Follow Position")]
		/// whether or not the object is currently following its target's position
		public bool FollowPosition = true;
		/// whether this object should follow its target on the X axis
		[MMCondition("FollowPosition", true)]
		public bool FollowPositionX = true;
		/// whether this object should follow its target on the Y axis
		[MMCondition("FollowPosition", true)]
		public bool FollowPositionY = true;
		/// whether this object should follow its target on the Z axis
		[MMCondition("FollowPosition", true)]
		public bool FollowPositionZ = true;
		/// whether to operate in world or local space
		[MMCondition("FollowPosition", true)] 
		public PositionSpaces PositionSpace = PositionSpaces.World;

		[Header("Follow Rotation")]
		/// whether or not the object is currently following its target's rotation
		public bool FollowRotation = true;

		[Header("Follow Scale")]
		/// whether or not the object is currently following its target's rotation
		public bool FollowScale = true;
		/// the factor to apply to the scale when following
		[MMCondition("FollowScale", true)]
		public float FollowScaleFactor = 1f;

		[Header("Target")]
		/// the target to follow
		public Transform Target;
		/// the offset to apply to the followed target
		[MMCondition("FollowPosition", true)]
		public Vector3 Offset;
		///whether or not to add the initial x distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceXToXOffset = false;
		///whether or not to add the initial y distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceYToYOffset = false;
		///whether or not to add the initial z distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceZToZOffset = false;

		[Header("Position Interpolation")]
		/// whether or not we need to interpolate the movement
		public bool InterpolatePosition = true;
		/// the follow mode to use when following position
		[MMCondition("InterpolatePosition", true)]
		public FollowModes FollowPositionMode = FollowModes.MMLerp;
		/// the speed at which to interpolate the follower's movement
		[MMCondition("InterpolatePosition", true)]
		public float FollowPositionSpeed = 10f;
		/// higher values mean more damping, less spring, low values mean less damping, more spring
		[MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)] 
		[Range(0.01f, 1.0f)]
		public float PositionSpringDamping = 0.3f;
		/// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
		[MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)]
		public float PositionSpringFrequency = 3f;

		[Header("Rotation Interpolation")]
		/// whether or not we need to interpolate the movement
		public bool InterpolateRotation = true;
		/// the follow mode to use when interpolating the rotation
		[MMCondition("InterpolateRotation", true)]
		public FollowModes FollowRotationMode = FollowModes.MMLerp;
		/// the speed at which to interpolate the follower's rotation
		[MMCondition("InterpolateRotation", true)]
		public float FollowRotationSpeed = 10f;
		/// higher values mean more damping, less spring, low values mean less damping, more spring
		[MMEnumCondition("FollowRotationMode", (int)FollowModes.MMSpring)] 
		[Range(0.01f, 1.0f)]
		public float RotationSpringDamping = 0.3f;
		/// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
		[MMEnumCondition("FollowRotationMode", (int)FollowModes.MMSpring)]
		public float RotationSpringFrequency = 3f;

		[Header("Scale Interpolation")]
		/// whether or not we need to interpolate the scale
		public bool InterpolateScale = true;
		/// the follow mode to use when interpolating the scale
		[MMCondition("InterpolateScale", true)]
		public FollowModes FollowScaleMode = FollowModes.MMLerp;
		/// the speed at which to interpolate the follower's scale
		[MMCondition("InterpolateScale", true)]
		public float FollowScaleSpeed = 10f;
		/// higher values mean more damping, less spring, low values mean less damping, more spring
		[MMEnumCondition("FollowScaleMode", (int)FollowModes.MMSpring)] 
		[Range(0.01f, 1.0f)]
		public float ScaleSpringDamping = 0.3f;
		/// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
		[MMEnumCondition("FollowScaleMode", (int)FollowModes.MMSpring)]
		public float ScaleSpringFrequency = 3f;

		[Header("Mode")]
		/// the update at which the movement happens
		public UpdateModes UpdateMode = UpdateModes.Update;
		/// if this is true, this component will self disable when its host game object gets disabled
		public bool DisableSelfOnSetActiveFalse = false;
        
		[Header("Distances")]
		/// whether or not to force a minimum distance between the object and its target before it starts following
		public bool UseMinimumDistanceBeforeFollow = false;
		/// the minimum distance to keep between the object and its target
		public float MinimumDistanceBeforeFollow = 1f;
		/// whether or not we want to make sure the object is never too far away from its target
		public bool UseMaximumDistance = false;
		/// the maximum distance at which the object can be away from its target
		public float MaximumDistance = 1f;

		[Header("Anchor")] 
		/// if this is true, the movement will be constrained around the initial position
		public bool AnchorToInitialPosition;
		/// the maximum distance around the initial position at which the transform can move
		[MMCondition("AnchorToInitialPosition", true)]
		public float MaxDistanceToAnchor = 1f;
		
		protected bool _localSpace { get { return PositionSpace == PositionSpaces.Local; } }

		protected Vector3 _positionVelocity = Vector3.zero;
		protected Vector3 _scaleVelocity = Vector3.zero;    
		protected Vector3 _rotationVelocity = Vector3.zero;    
		
		protected Vector3 _initialPosition;
		protected Vector3 _direction;
		
		protected Vector3 _newPosition;
		protected Vector3 _newRotation;
		protected Vector3 _newScale;
		
		protected Vector3 _newTargetPosition;    
		protected Quaternion _newTargetRotation;
		protected Vector3 _newTargetRotationEulerAngles;
		protected Vector3 _newTargetRotationEulerAnglesLastFrame;
		protected Vector3 _newTargetScale;

		protected float _rotationFloatVelocity;
		protected float _rotationFloatCurrent;
		protected float _rotationFloatTarget;

		protected Vector3 _currentRotationEulerAngles;
		protected Quaternion _rotationBeforeSpring;
		
		protected Quaternion _initialRotation;
		protected Vector3 _lastTargetPosition;
        
		/// <summary>
		/// On start we store our initial position
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Initializes the follow
		/// </summary>
		public virtual void Initialization()
		{
			SetInitialPosition();
			SetOffset();
		}

		/// <summary>
		/// Prevents the object from following the target anymore
		/// </summary>
		public virtual void StopFollowing()
		{
			FollowPosition = false;
		}

		/// <summary>
		/// Makes the object follow the target
		/// </summary>
		public virtual void StartFollowing()
		{
			FollowPosition = true;
			SetInitialPosition();
		}

		/// <summary>
		/// Stores the initial position
		/// </summary>
		protected virtual void SetInitialPosition()
		{
			_initialPosition = _localSpace ? this.transform.localPosition : this.transform.position;
			_initialRotation = this.transform.rotation;
			_lastTargetPosition = _localSpace ? this.transform.localPosition : this.transform.position;
		}

		/// <summary>
		/// Adds initial offset to the offset if needed
		/// </summary>
		protected virtual void SetOffset()
		{
			if (Target == null)
			{
				return;
			}
			Vector3 difference = this.transform.position - Target.transform.position;
			Offset.x = AddInitialDistanceXToXOffset ? difference.x : Offset.x;
			Offset.y = AddInitialDistanceYToYOffset ? difference.y : Offset.y;
			Offset.z = AddInitialDistanceZToZOffset ? difference.z : Offset.z;
		}

		/// <summary>
		/// At update we follow our target 
		/// </summary>
		protected virtual void Update()
		{
			if (Target == null)
			{
				return;
			}
			if (UpdateMode == UpdateModes.Update)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At fixed update we follow our target 
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At late update we follow our target 
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// Follows the target, lerping the position or not based on what's been defined in the inspector
		/// </summary>
		protected virtual void FollowTargetPosition()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowPosition)
			{
				return;
			}

			_newTargetPosition = Target.position + Offset;
			if (!FollowPositionX) { _newTargetPosition.x = _initialPosition.x; }
			if (!FollowPositionY) { _newTargetPosition.y = _initialPosition.y; }
			if (!FollowPositionZ) { _newTargetPosition.z = _initialPosition.z; }

			float trueDistance = 0f;
			_direction = (_newTargetPosition - this.transform.position).normalized;
			trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);
            
			float interpolatedDistance = trueDistance;
			if (InterpolatePosition)
			{
				switch (FollowPositionMode)
				{
					case FollowModes.MMLerp:
						interpolatedDistance = MMMaths.Lerp(0f, trueDistance, FollowPositionSpeed, Time.deltaTime);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.RegularLerp:
						interpolatedDistance = Mathf.Lerp(0f, trueDistance, Time.deltaTime * FollowPositionSpeed);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.MMSpring:
						_newPosition = this.transform.position;
						MMMaths.Spring(ref _newPosition, _newTargetPosition, ref _positionVelocity, PositionSpringDamping, PositionSpringFrequency, Time.deltaTime);
						if (_localSpace)
						{
							this.transform.localPosition = _newPosition;   
						}
						else
						{
							this.transform.position = _newPosition;    
						}
						break;
				}                
			}
			else
			{
				interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}

			if (AnchorToInitialPosition)
			{
				if (Vector3.Distance(this.transform.position, _initialPosition) > MaxDistanceToAnchor)
				{
					if (_localSpace)
					{
						this.transform.localPosition = _initialPosition + Vector3.ClampMagnitude(this.transform.localPosition - _initialPosition, MaxDistanceToAnchor);   
					}
					else
					{
						this.transform.position = _initialPosition + Vector3.ClampMagnitude(this.transform.position - _initialPosition, MaxDistanceToAnchor);    
					}
				}
			}
		}

		/// <summary>
		/// Applies minimal and maximal distance rules to the interpolated distance
		/// </summary>
		/// <param name="trueDistance"></param>
		/// <param name="interpolatedDistance"></param>
		/// <returns></returns>
		protected virtual float ApplyMinMaxDistancing(float trueDistance, float interpolatedDistance)
		{
			if (UseMinimumDistanceBeforeFollow && (trueDistance - interpolatedDistance < MinimumDistanceBeforeFollow))
			{
				interpolatedDistance = 0f;
			}

			if (UseMaximumDistance && (trueDistance - interpolatedDistance >= MaximumDistance))
			{
				interpolatedDistance = trueDistance - MaximumDistance;
			}

			return interpolatedDistance;
		}
		
		/// <summary>
		/// Makes the object follow its target's rotation
		/// </summary>
		protected virtual void FollowTargetRotation()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowRotation)
			{
				return;
			}

			_newTargetRotation = Target.rotation;
			
			_newTargetRotationEulerAngles = Target.rotation.eulerAngles;
			_currentRotationEulerAngles = this.transform.rotation.eulerAngles;
			
			if (FollowRotationMode == FollowModes.MMSpring && (_newTargetRotationEulerAnglesLastFrame != _newTargetRotationEulerAngles))
			{
				_rotationBeforeSpring = this.transform.rotation;
				_rotationFloatCurrent = 0f;
				_rotationFloatTarget = (Mathf.Abs(_newTargetRotation.eulerAngles.x)
				                        + Mathf.Abs(_newTargetRotation.eulerAngles.y)
				                        + Mathf.Abs(_newTargetRotation.z))
				                       -
				                       (Mathf.Abs(_currentRotationEulerAngles.x)
				                        + Mathf.Abs(_currentRotationEulerAngles.y)
				                        + Mathf.Abs(_currentRotationEulerAngles.z));

				_rotationFloatTarget = Mathf.Abs(_rotationFloatTarget);
			}

			if (InterpolateRotation)
			{
				switch (FollowRotationMode)
				{
					case FollowModes.MMLerp:
						this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _newTargetRotation, Time.deltaTime * FollowRotationSpeed);
						break;
					case FollowModes.MMSpring:
						if (_rotationFloatCurrent == _rotationFloatTarget)
						{
							break;
						}
						MMMaths.Spring(ref _rotationFloatCurrent, _rotationFloatTarget, ref _rotationFloatVelocity, RotationSpringDamping, RotationSpringFrequency, Time.deltaTime);
						float lerpValue = MMMaths.Remap(_rotationFloatCurrent, 0f, _rotationFloatTarget, 0f, 1f);
						this.transform.rotation = Quaternion.LerpUnclamped(_rotationBeforeSpring, _newTargetRotation, lerpValue );   
						break;
				}
			}
			else
			{
				this.transform.rotation = _newTargetRotation;
			}

			_newTargetRotationEulerAnglesLastFrame = _newTargetRotationEulerAngles;
		}

		/// <summary>
		/// Makes the object follow its target's scale
		/// </summary>
		protected virtual void FollowTargetScale()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowScale)
			{
				return;
			}

			_newTargetScale = Target.localScale * FollowScaleFactor;

			if (InterpolateScale)
			{
				switch (FollowScaleMode)
				{
					case FollowModes.MMLerp:
						this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newTargetScale, FollowScaleSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.localScale = Vector3.Lerp(this.transform.localScale, _newTargetScale, Time.deltaTime * FollowScaleSpeed);
						break;
					case FollowModes.MMSpring:
						_newScale = this.transform.localScale;
						MMMaths.Spring(ref _newScale, _newTargetScale, ref _scaleVelocity, ScaleSpringDamping, ScaleSpringFrequency, Time.deltaTime);
						this.transform.localScale = _newScale;   
						break;
				}
			}
			else
			{
				this.transform.localScale = _newTargetScale;
			}
		}
        
		public virtual void ChangeFollowTarget(Transform newTarget) => Target = newTarget;

		protected virtual void OnDisable()
		{
			if (DisableSelfOnSetActiveFalse)
			{
				this.enabled = false;
			}
		}
	}
}