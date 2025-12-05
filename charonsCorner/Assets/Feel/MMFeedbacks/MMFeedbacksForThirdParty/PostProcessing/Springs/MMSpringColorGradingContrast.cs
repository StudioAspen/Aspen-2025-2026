#if MM_POSTPROCESSING
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Color Grading Contrast")]
	public class MMSpringColorGradingContrast : MMSpringFloatComponent<PostProcessVolume>
	{
		protected ColorGrading _colorGrading;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<PostProcessVolume>();
			}
			Target.profile.TryGetSettings(out _colorGrading);
			base.Initialization();
		}
		
		public override float TargetFloat
		{
			get => _colorGrading.contrast;
			set => _colorGrading.contrast.Override(value);
		}
	}
}
#endif