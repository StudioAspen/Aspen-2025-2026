using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Light Intensity")]
	public class MMSpringLightIntensity : MMSpringFloatComponent<Light>
	{
		public override float TargetFloat
		{
			get => Target.intensity;
			set => Target.intensity = value; 
		}
	}
}
