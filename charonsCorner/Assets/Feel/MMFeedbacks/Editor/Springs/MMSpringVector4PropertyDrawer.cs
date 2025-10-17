using MoreMountains.Tools;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MoreMountains.Feedbacks
{
	[CustomPropertyDrawer(typeof(MMSpringVector4))]
	class MMSpringVector4PropertyDrawer : PropertyDrawer
	{
		protected float _lastTarget;
		protected float _max;
		
		protected PropertyField _unifiedSpringField;
		protected Label _springXLabel;
		protected PropertyField _springXField;
		protected Label _springYLabel;
		protected PropertyField _springYField;
		protected Label _springZLabel;
		protected PropertyField _springZField;
		protected Label _springWLabel;
		protected PropertyField _springWField;
		
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var root = new VisualElement();
			
			SerializedProperty _separateAxis = property.FindPropertyRelative("SeparateAxis");
			SerializedProperty _unifiedSpring = property.FindPropertyRelative("UnifiedSpring");
			SerializedProperty _springX = property.FindPropertyRelative("SpringX");
			SerializedProperty _springY = property.FindPropertyRelative("SpringY");
			SerializedProperty _springZ = property.FindPropertyRelative("SpringZ");
			SerializedProperty _springW = property.FindPropertyRelative("SpringW");
   
			Toggle boolToggle = new Toggle("SeparateAxis") { value = property.FindPropertyRelative("SeparateAxis").boolValue };
			boolToggle.RegisterValueChangedCallback(evt =>
			{
				property.FindPropertyRelative("SeparateAxis").boolValue = evt.newValue;
				ToggleFields(evt.newValue);
				_separateAxis.serializedObject.ApplyModifiedProperties();
			});
			root.Add(boolToggle);

			_unifiedSpringField = new PropertyField(_unifiedSpring);
			
			_springXLabel = new Label("Spring X");
			_springXLabel.style.backgroundColor = new StyleColor(new Color(1f,0,0,0.2f));
			_springXLabel.style.marginLeft = -10;
			_springXLabel.style.paddingLeft = 14;
			_springXLabel.style.paddingBottom = 3;
			_springXLabel.style.paddingTop = 3;
			_springXField = new PropertyField(_springX);
			
			_springYLabel = new Label("Spring Y");
			_springYLabel.style.backgroundColor = new StyleColor(new Color(0,1f,0,0.1f));
			_springYLabel.style.marginLeft = -10;
			_springYLabel.style.paddingLeft = 14;
			_springYLabel.style.paddingBottom = 3;
			_springYLabel.style.paddingTop = 3;
			_springYField = new PropertyField(_springY);
			
			_springZLabel = new Label("Spring Z");
			_springZLabel.style.backgroundColor = new StyleColor(new Color(0,0.4f,0.9f,0.2f));
			_springZLabel.style.marginLeft = -10;
			_springZLabel.style.paddingLeft = 14;
			_springZLabel.style.paddingBottom = 3;
			_springZLabel.style.paddingTop = 3;
			_springZField = new PropertyField(_springZ);
			
			_springWLabel = new Label("Spring W");
			_springWLabel.style.backgroundColor = new StyleColor(new Color(0.7f,0f,0.7f,0.2f));
			_springWLabel.style.marginLeft = -10;
			_springWLabel.style.paddingLeft = 14;
			_springWLabel.style.paddingBottom = 3;
			_springWLabel.style.paddingTop = 3;
			_springWField = new PropertyField(_springW);
			
			root.Add(_unifiedSpringField);
			root.Add(_springXLabel);
			root.Add(_springXField);
			root.Add(_springYLabel);
			root.Add(_springYField);
			root.Add(_springZLabel);
			root.Add(_springZField);
			root.Add(_springWLabel);
			root.Add(_springWField);

			ToggleFields(boolToggle.value);
			
			return root;
		}
		
		private void ToggleFields(bool show)
		{
			_unifiedSpringField.style.display = show ? DisplayStyle.None : DisplayStyle.Flex;
			_springXLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springXField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springYLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springYField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springZLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springZField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springWLabel.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
			_springWField.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
		}
		
	}
}


