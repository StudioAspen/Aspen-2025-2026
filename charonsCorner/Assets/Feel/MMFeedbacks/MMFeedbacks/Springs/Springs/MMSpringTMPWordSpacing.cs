using UnityEngine;
#if MM_UGUI2
using TMPro;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring TMP Word Spacing")]
	public class MMSpringTMPWordSpacing : MMSpringFloatComponent<TMP_Text>
	{
		public override float TargetFloat
		{
			get => Target.wordSpacing;
			set => Target.wordSpacing = value;
		}
	}
}
#endif
