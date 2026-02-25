using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Camera Field Of View")]
	public class MMSpringCameraFieldOfView : MMSpringFloatComponent<Camera>
	{
		public override float TargetFloat
		{
			get => Target.fieldOfView;
			set => Target.fieldOfView = value;
		}
	}
}
