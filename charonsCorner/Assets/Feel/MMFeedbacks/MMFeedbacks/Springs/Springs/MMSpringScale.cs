using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Scale")]
	public class MMSpringScale : MMSpringVector3Component<Transform>
	{
		protected override void Initialization()
		{
			base.Initialization();
			
			SpringVector3.SpringX.ClampSettings.ClampMin = true;
			SpringVector3.SpringX.ClampSettings.ClampMinValue = 0f;
			SpringVector3.SpringX.ClampSettings.ClampMinBounce = true;
			
			SpringVector3.SpringY.ClampSettings.ClampMin = true;
			SpringVector3.SpringY.ClampSettings.ClampMinValue = 0f;
			SpringVector3.SpringY.ClampSettings.ClampMinBounce = true;
			
			SpringVector3.SpringZ.ClampSettings.ClampMin = true;
			SpringVector3.SpringZ.ClampSettings.ClampMinValue = 0f;
			SpringVector3.SpringZ.ClampSettings.ClampMinBounce = true;
		}

		public override Vector3 TargetVector3
		{
			get => Target.localScale;
			set => Target.localScale = value;
		}
	}
}
