using MoreMountains.Tools;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MoreMountains.Feedbacks
{
	[CustomPropertyDrawer(typeof(MMSpringFloat))]
	class MMSpringFloatPropertyDrawer : PropertyDrawer
	{
		protected float _lastTarget;
		protected float _max;
		
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var root = new VisualElement();
			
			root.style.backgroundColor = new StyleColor(new Color(255,255,255,0.03f));
			root.style.marginTop = 5;
			root.style.paddingRight = 5;
			
			SerializedProperty _damping = property.FindPropertyRelative("Damping");
			SerializedProperty _frequency = property.FindPropertyRelative("Frequency");
			SerializedProperty _unifiedSpring = property.FindPropertyRelative("UnifiedSpring");
			SerializedProperty _springDebug = property.FindPropertyRelative("SpringDebug");
			
			root.Add(new PropertyField(_damping));
			root.Add(new PropertyField(_frequency));
			
			if (!_unifiedSpring.boolValue)
			{
				SerializedProperty _clampSettings = property.FindPropertyRelative("ClampSettings");
				root.Add(new PropertyField(_clampSettings));
			}

			if (Application.isPlaying && !_unifiedSpring.boolValue)
			{
				VisualElement horizontalLayout = new VisualElement();
				horizontalLayout.style.flexDirection = FlexDirection.Row;
				root.Add(horizontalLayout);

				FloatField currentValue = new FloatField("CurrentValue") { bindingPath = "CurrentValueDisplay", isReadOnly = true, style = { flexGrow = 1, paddingRight = 10 } };
				currentValue.SetEnabled(false);
				currentValue.AddToClassList("mm-fixed-width-floatfield");
				horizontalLayout.Add(currentValue);
				
				FloatField targetValue = new FloatField("TargetValue") { bindingPath = "TargetValueDisplay", isReadOnly = true, style = { flexGrow = 1} };
				targetValue.SetEnabled(false);
				targetValue.AddToClassList("mm-fixed-width-floatfield");
				horizontalLayout.Add(targetValue);
				
				FloatField velocity = new FloatField("Velocity") { bindingPath = "VelocityDisplay", isReadOnly = true, style = { flexGrow = 1, paddingLeft = 10} };
				velocity.SetEnabled(false);
				velocity.AddToClassList("mm-fixed-width-floatfield");
				horizontalLayout.Add(velocity);
				
				root.Add(new PropertyField(_springDebug));
			}

			return root;
		}
		
	}
}


