using UnityEngine;
#if MM_HDRP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Vignette Center HDRP")]
	public class MMSpringVignetteCenter_HDRP : MMSpringVector2Component<Volume>
	{
		protected Vignette _vignette;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<Volume>();
			}
			Target.profile.TryGet(out _vignette);
			base.Initialization();
		}
		
		public override Vector2 TargetVector2
		{
			get => _vignette.center.value;
			set => _vignette.center.Override(value);
		}
	}
}
#endif