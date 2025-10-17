#if MM_HDRP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring White Balance Tint HDRP")]
	public class MMSpringWhiteBalanceTint_HDRP : MMSpringFloatComponent<Volume>
	{
		protected WhiteBalance _whiteBalance;
		
		protected override void Initialization()
		{
			if (Target == null)
			{
				Target = this.gameObject.GetComponent<Volume>();
			}
			Target.profile.TryGet(out _whiteBalance);
			base.Initialization();
		}
		
		public override float TargetFloat
		{
			get => _whiteBalance.tint.value;
			set => _whiteBalance.tint.Override(value);
		}
	}
}
#endif