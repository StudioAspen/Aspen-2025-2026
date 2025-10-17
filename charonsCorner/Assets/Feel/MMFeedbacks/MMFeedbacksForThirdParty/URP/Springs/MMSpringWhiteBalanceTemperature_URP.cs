#if MM_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring White Balance Temperature URP")]
	public class MMSpringWhiteBalanceTemperature_URP : MMSpringFloatComponent<Volume>
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
			get => _whiteBalance.temperature.value;
			set => _whiteBalance.temperature.Override(value);
		}
	}
}
#endif