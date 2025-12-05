using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMTweenType))]
	public class MMTweenTypeDrawer : PropertyDrawer
	{
		protected const int _lineHeight = 20; 
		protected const int _lineMargin = 2;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!ShouldDisplay(property))
			{
				return 0;
			}
			
			return _lineHeight * 2 + _lineMargin;
		}

		#if  UNITY_EDITOR
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var type = property.FindPropertyRelative("MMTweenDefinitionType");

			if (!ShouldDisplay(property))
			{
				return;
			}
            
			EditorGUI.BeginProperty(position, label, property);

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            
			var definitionTypeRect = new Rect(position.x, position.y, position.width, _lineHeight);
			var curveRect = new Rect(position.x, position.y + _lineHeight + _lineMargin, position.width, _lineHeight);

			EditorGUI.PropertyField(definitionTypeRect, property.FindPropertyRelative("MMTweenDefinitionType"), GUIContent.none);
			if (type.enumValueIndex == 0)
			{
				EditorGUI.PropertyField(curveRect, property.FindPropertyRelative("MMTweenCurve"), GUIContent.none);
			}
			if (type.enumValueIndex == 1)
			{
				EditorGUI.PropertyField(curveRect, property.FindPropertyRelative("Curve"), GUIContent.none);
			}
            
			EditorGUI.EndProperty();
		}

		protected virtual bool ShouldDisplay(SerializedProperty property)
		{
			string conditionPropertyName = property.FindPropertyRelative("ConditionPropertyName").stringValue;
			
			if (!string.IsNullOrEmpty(conditionPropertyName))
			{
				string propertyPath = property.propertyPath;
				string conditionPath = propertyPath.Replace(property.name, conditionPropertyName);
				SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

				if (sourcePropertyValue != null)
				{
					return sourcePropertyValue.boolValue;
				}
			}
			
			string enumConditionPropertyName = property.FindPropertyRelative("EnumConditionPropertyName").stringValue;
			if (!string.IsNullOrEmpty(enumConditionPropertyName))
			{
				string propertyPath = property.propertyPath;
				string conditionPath = propertyPath.Replace(property.name, enumConditionPropertyName);
				SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

				if (sourcePropertyValue != null)
				{
					SerializedProperty enumProperty = property.FindPropertyRelative("EnumConditions");
					if (enumProperty.arraySize > sourcePropertyValue.enumValueIndex)
					{
						return enumProperty.GetArrayElementAtIndex(sourcePropertyValue.enumValueIndex).boolValue;	
					}
				}
			}

			return true;
		}
		
		#endif
	}
}