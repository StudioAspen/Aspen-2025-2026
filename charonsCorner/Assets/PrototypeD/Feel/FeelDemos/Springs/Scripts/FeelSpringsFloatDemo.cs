using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine;

namespace MoreMountains.Feel
{
	[AddComponentMenu("")]
	public class FeelSpringsFloatDemo : MonoBehaviour
	{
		[Header("Spring")]
		public MMSpringFloat FloatSpring;

		[Header("Bindings")] 
		public FeelSpringsDemoSlider DampingSlider;
		public FeelSpringsDemoSlider FrequencySlider;
		public FeelSpringsDemoSlider BumpAmountSlider;
		public Transform MovingObject;

		protected Vector3 _newPosition;
		protected float _range = 0.375f;

		protected virtual void OnEnable()
		{
			FloatSpring.CurrentValue = 0f;
			FloatSpring.TargetValue = 0f;
			FloatSpring.Velocity = 0f;
		}
		
		public virtual void RandomMove()
		{
			FloatSpring.MoveTo(UnityEngine.Random.Range(-1f,1f));
		}

		public virtual void RandomBump()
		{
			float bumpAmount = BumpAmountSlider.value;
			FloatSpring.BumpRandom(-bumpAmount, bumpAmount);
		}
		
		protected virtual void Update()
		{
			FloatSpring.Damping = DampingSlider.value;
			FloatSpring.Frequency = FrequencySlider.value;
			FloatSpring.UpdateSpringValue(Time.deltaTime);

			_newPosition = MovingObject.transform.localPosition;
			_newPosition.x = MMMaths.Remap(FloatSpring.CurrentValue, -1f, 1f, -_range, _range);

			MovingObject.transform.localPosition = _newPosition;
		}
	}
}
