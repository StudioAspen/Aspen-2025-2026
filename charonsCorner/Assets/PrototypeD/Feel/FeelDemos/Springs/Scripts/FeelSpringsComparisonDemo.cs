using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine;

namespace MoreMountains.Feel
{
	[AddComponentMenu("")]
	public class FeelSpringsComparisonDemo : MonoBehaviour
	{
		[Header("Spring")]
		public List<MMSpringFloat> Springs;
		public List<Transform> MovingObjects;
		public FeelSpringsDemoSlider BumpAmountSlider;

		protected Vector3 _newPosition;
		
		protected float _range = 0.375f;

		protected virtual void OnEnable()
		{
			foreach (MMSpringFloat spring in Springs)
			{
				spring.CurrentValue = 0f;
				spring.TargetValue = 0f;
				spring.Velocity = 0f;	
			}
		}

		public virtual void RandomBump()
		{
			float bumpAmount = BumpAmountSlider.value;
			foreach (MMSpringFloat spring in Springs)
			{
				spring.Bump(bumpAmount);
			}
		}
		
		protected virtual void Update()
		{
			for (int i = 0; i < Springs.Count; i++)
			{
				Springs[i].UpdateSpringValue(Time.deltaTime);
				
				_newPosition = MovingObjects[i].transform.localPosition;
				_newPosition.x = MMMaths.Remap(Springs[i].CurrentValue, -1f, 1f, -_range, _range);
				MovingObjects[i].transform.localPosition = _newPosition;
			}
		}
	}
}
