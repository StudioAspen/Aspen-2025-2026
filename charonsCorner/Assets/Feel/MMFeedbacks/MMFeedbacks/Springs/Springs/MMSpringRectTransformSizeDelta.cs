using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Rect Transform Size Delta")]
	public class MMSpringRectTransformSizeDelta : MMSpringVector2Component<RectTransform>
	{
		public override Vector2 TargetVector2
		{
			get => Target.sizeDelta;
			set => Target.sizeDelta = value;
		}
	}
}
