using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Animator Speed")]
	public class MMSpringAnimatorSpeed : MMSpringFloatComponent<Animator>
	{
		public override float TargetFloat
		{
			get => Target.speed;
			set => Target.speed = Mathf.Abs(value); 
		}
	}
}
