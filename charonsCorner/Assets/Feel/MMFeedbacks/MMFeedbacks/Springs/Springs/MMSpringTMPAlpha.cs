using UnityEngine;
#if MM_UGUI2
using TMPro;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring TMP Alpha")]
	public class MMSpringTMPAlpha : MMSpringFloatComponent<TMP_Text>
	{
		public override float TargetFloat
		{
			get => Target.alpha;
			set => Target.alpha = value;
		}
	}
}
#endif
