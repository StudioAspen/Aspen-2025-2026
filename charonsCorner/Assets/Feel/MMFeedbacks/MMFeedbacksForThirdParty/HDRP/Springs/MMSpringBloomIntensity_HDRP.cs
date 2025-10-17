#if MM_HDRP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Bloom Intensity HDRP")]
	public class MMSpringBloomIntensity_HDRP : MMSpringFloatComponent<Volume>
	{
		protected Bloom _bloom;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<Volume>();
			}
			Target.profile.TryGet(out _bloom);
			base.Initialization();
		}
		
		public override float TargetFloat
		{
			get => _bloom.intensity.value;
			set => _bloom.intensity.Override(value);
		}
	}
}
#endif