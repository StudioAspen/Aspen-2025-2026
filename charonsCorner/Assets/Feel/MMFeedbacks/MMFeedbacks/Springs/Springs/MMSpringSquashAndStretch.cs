using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("More Mountains/Springs/MM Spring Squash And Stretch")]
	public class MMSpringSquashAndStretch : MMSpringFloatComponent<Transform>
	{
		public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }

		[MMInspectorGroup("Target", true, 17)] 
		public PossibleAxis Axis = PossibleAxis.XtoYZ;

		protected Vector3 _newScale;
		protected Vector3 _initialScale;
		
		protected override void Initialization()
		{
			base.Initialization();
			FloatSpring.ClampSettings.ClampMin = true;
			FloatSpring.ClampSettings.ClampMinValue = 0f;
			FloatSpring.ClampSettings.ClampMinBounce = true;
			_initialScale = Target.localScale;
		}
		
		protected override void ApplyValue(float newValue)
		{
			float invertScale = 1 / Mathf.Sqrt(newValue);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newValue;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newValue;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newValue;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newValue;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newValue;
					break;
			}
			Target.localScale = _newScale;
		}
		
		protected override void GrabCurrentValue()
		{
			FloatSpring.CurrentValue = Target.localScale.x;
		}
	}
}
