#if MM_UI
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Image Fill Amount")]
	public class MMSpringImageFillAmount : MMSpringFloatComponent<Image>
	{
		public override float TargetFloat
		{
			get => Target.fillAmount;
			set => Target.fillAmount = value;
		}
	}
}
#endif