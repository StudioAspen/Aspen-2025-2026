using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Rotation")]
	public class MMSpringRotation : MMSpringVector3Component<Transform>
	{
		public enum Spaces { Local, World }

		[MMInspectorGroup("Target", true, 17)] 
		public Spaces Space = Spaces.World;
		
		public override Vector3 TargetVector3
		{
			get => (Space == Spaces.Local) ? Target.localRotation.eulerAngles : Target.rotation.eulerAngles;
			set
			{
				if (Space == Spaces.Local)
				{
					Target.localRotation = Quaternion.Euler(value);
				}
				else
				{
					Target.rotation = Quaternion.Euler(value);
				}
			}
		}
	}
}
