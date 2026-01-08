using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Rotation Around")]
	public class MMSpringRotationAround : MMSpringFloatComponent<Transform>
	{
		public Transform RotationCenter;
		public Vector3 RotationAxis = Vector3.up;
		public bool FaceRotationCenter = true;

		protected float _currentAngle;
		protected Vector3 _initialPosition;
		protected Quaternion _initialRotation;
		
		protected override void Initialization()
		{
			base.Initialization();
			_currentAngle = 0;
			_initialPosition = Target.position;
			_initialRotation = Target.rotation;
		}
		
		
		public override float TargetFloat
		{
			get
			{
				return _currentAngle;
			}
			set
			{
				_currentAngle = value;
				Target.position = _initialPosition;
				Target.rotation = _initialRotation;
				Target.position = MMMaths.RotatePointAroundPivot(Target.position, RotationCenter.position, _currentAngle * RotationAxis);
				if (FaceRotationCenter)
				{
					Target.LookAt(RotationCenter);
				}
			}
		}
	}
}
