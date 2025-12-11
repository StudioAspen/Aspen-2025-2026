#if MM_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Motion Blur Intensity URP")]
	public class MMSpringMotionBlurIntensity_URP : MMSpringFloatComponent<Volume>
	{
		protected MotionBlur _motionBlur;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<Volume>();
			}
			Target.profile.TryGet(out _motionBlur);
			base.Initialization();
		}
		
		public override float TargetFloat
		{
			get => _motionBlur.intensity.value;
			set => _motionBlur.intensity.Override(value);
		}
	}
}
#endif