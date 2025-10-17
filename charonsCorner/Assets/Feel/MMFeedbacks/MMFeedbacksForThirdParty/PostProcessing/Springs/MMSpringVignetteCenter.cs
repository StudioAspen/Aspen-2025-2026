using UnityEngine;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Vignette Center")]
	public class MMSpringVignetteCenter : MMSpringVector2Component<PostProcessVolume>
	{
		protected Vignette _vignette;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<PostProcessVolume>();
			}
			Target.profile.TryGetSettings(out _vignette);
			base.Initialization();
		}
		
		public override Vector2 TargetVector2
		{
			get => _vignette.center;
			set => _vignette.center.Override(value);
		}
	}
}
#endif