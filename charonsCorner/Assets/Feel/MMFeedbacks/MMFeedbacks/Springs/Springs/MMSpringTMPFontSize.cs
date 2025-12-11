using UnityEngine;
#if MM_UGUI2
using TMPro;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring TMP Font Size")]
	public class MMSpringTMPFontSize : MMSpringFloatComponent<TMP_Text>
	{
		public override float TargetFloat
		{
			get => Target.fontSize;
			set => Target.fontSize = (int)value;
		}
	}
}
#endif
