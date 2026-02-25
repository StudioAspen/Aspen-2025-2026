#if MM_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Lens Distortion Intensity URP")]
	public class MMSpringLensDistortionIntensity_URP : MMSpringFloatComponent<Volume>
	{
		protected LensDistortion _lensDistortion;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<Volume>();
			}
			Target.profile.TryGet(out _lensDistortion);
			base.Initialization();
		}
		
		public override float TargetFloat
		{
			get => _lensDistortion.intensity.value;
			set => _lensDistortion.intensity.Override(value);
		}
	}
}
#endif