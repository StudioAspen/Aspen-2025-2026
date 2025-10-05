using MoreMountains.Feedbacks;
#if MM_UGUI2
using TMPro;
#endif
using UnityEngine;

namespace MoreMountains.Feel
{
	[AddComponentMenu("")]
	public class FeelSpringsAdvancedFloatDemo : MonoBehaviour
	{
		[Header("Bindings")] 
		public MMSpringPosition PositionSpring;
		public MMSpringRotation RotationSpring;
		public MMSpringScale ScaleSpring;
		
		public FeelSpringsDemoSlider PositionDampingSlider;
		public FeelSpringsDemoSlider PositionFrequencySlider;
		public FeelSpringsDemoSlider RotationDampingSlider;
		public FeelSpringsDemoSlider RotationFrequencySlider;
		public FeelSpringsDemoSlider ScaleDampingSlider;
		public FeelSpringsDemoSlider ScaleFrequencySlider;

		
		public FeelSpringsDemoSlider BumpAmountSlider;
		
		public Transform MovingObject;

		protected Vector3 _newPosition = Vector3.zero;
		protected Vector3 _newBump = Vector3.zero;
		protected float _range = 0.375f;

		protected virtual void Awake()
		{
			_newPosition = MovingObject.transform.localPosition;
		}
		
		public virtual void RandomMove()
		{
			_newPosition.x = UnityEngine.Random.Range(-_range, _range);
			PositionSpring.MoveTo(_newPosition);
			ScaleSpring.BumpRandom();
			RotationSpring.BumpRandom();
		}

		public virtual void RandomBump()
		{
			_newBump.x = 0f;
			_newBump.y = 0f;
			_newBump.z = 1000f * BumpAmountSlider.value;
			RotationSpring.Bump(_newBump);
			_newBump.x = 10f * BumpAmountSlider.value;
			_newBump.y = 10f * BumpAmountSlider.value;
			_newBump.z = 0f;
			ScaleSpring.Bump(_newBump);
		}
		
		protected virtual void Update()
		{
			PositionSpring.SpringVector3.SetDamping(PositionDampingSlider.value * Vector3.one);
			PositionSpring.SpringVector3.SetFrequency(PositionFrequencySlider.value * Vector3.one);
			
			RotationSpring.SpringVector3.SetDamping(RotationDampingSlider.value * Vector3.one);
			RotationSpring.SpringVector3.SetFrequency(RotationFrequencySlider.value * Vector3.one);
			
			ScaleSpring.SpringVector3.SetDamping(ScaleDampingSlider.value * Vector3.one);
			ScaleSpring.SpringVector3.SetFrequency(ScaleFrequencySlider.value * Vector3.one);
		}
	}
}
