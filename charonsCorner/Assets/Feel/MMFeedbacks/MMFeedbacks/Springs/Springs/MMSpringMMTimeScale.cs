using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring MMTimeScale")]
	public class MMSpringMMTimeScale : MMSpringFloatComponent<Transform>
	{
		protected override void Initialization()
		{
			base.Initialization();
			FloatSpring.ClampSettings.ClampMin = true;
			FloatSpring.ClampSettings.ClampMinValue = 0f;
			FloatSpring.ClampSettings.ClampMinBounce = true;
		}

		public override float TargetFloat
		{
			get => MMTimeManager.Instance.CurrentTimeScale;
			set => MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, value, 0f, false, 0f, true);
		}
	}
}
