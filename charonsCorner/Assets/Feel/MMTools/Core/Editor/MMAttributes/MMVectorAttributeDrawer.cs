using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
	[CustomPropertyDrawer(typeof(MMVectorAttribute))]
	public class MMVectorLabelsAttributeDrawer : PropertyDrawer
	{
		protected static readonly GUIContent[] originalLabels = new GUIContent[] { new GUIContent("X"), new GUIContent("Y"), new GUIContent("Z"), new GUIContent("W") };
		protected const int padding = 375;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent guiContent)
		{
			int ratio = (padding > Screen.width) ? 2 : 1;
			return ratio * base.GetPropertyHeight(property, guiContent);
		}
        
		#if UNITY_EDITOR
		public override void OnGUI(Rect rect, SerializedProperty property, GUIContent guiContent)
		{
			MMVectorAttribute vector = (MMVectorAttribute)attribute;
            
			EditorGUI.BeginProperty(rect, guiContent, property);
			
			// Use child properties instead of direct vector access
			if (property.propertyType == SerializedPropertyType.Vector2)
			{
				SerializedProperty[] props = new SerializedProperty[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y") };
				DrawFieldsWithProperties(rect, props, ObjectNames.NicifyVariableName(property.name), vector, guiContent);
			}
			else if (property.propertyType == SerializedPropertyType.Vector3)
			{
				SerializedProperty[] props = new SerializedProperty[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y"), property.FindPropertyRelative("z") };
				DrawFieldsWithProperties(rect, props, ObjectNames.NicifyVariableName(property.name), vector, guiContent);
			}
			else if (property.propertyType == SerializedPropertyType.Vector4)
			{
				SerializedProperty[] props = new SerializedProperty[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y"), property.FindPropertyRelative("z"), property.FindPropertyRelative("w") };
				DrawFieldsWithProperties(rect, props, ObjectNames.NicifyVariableName(property.name), vector, guiContent);
			}
			else if (property.propertyType == SerializedPropertyType.Vector2Int)
			{
				SerializedProperty[] props = new SerializedProperty[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y") };
				DrawFieldsWithProperties(rect, props, ObjectNames.NicifyVariableName(property.name), vector, guiContent);
			}
			else if (property.propertyType == SerializedPropertyType.Vector3Int)
			{
				SerializedProperty[] props = new SerializedProperty[] { property.FindPropertyRelative("x"), property.FindPropertyRelative("y"), property.FindPropertyRelative("z") };
				DrawFieldsWithProperties(rect, props, ObjectNames.NicifyVariableName(property.name), vector, guiContent);
			}
			
			EditorGUI.EndProperty();
		}
		#endif

		protected void DrawFieldsWithProperties(Rect rect, SerializedProperty[] properties, string mainLabel, MMVectorAttribute vectors, GUIContent originalGuiContent)
		{
			bool shortSpace = (Screen.width < padding);

			Rect mainLabelRect = rect;
			mainLabelRect.width = EditorGUIUtility.labelWidth;
			if (shortSpace)
			{
				mainLabelRect.height *= 0.5f;
			}                

			Rect fieldRect = rect;
			if (shortSpace)
			{
				fieldRect.height *= 0.5f;
				fieldRect.y += fieldRect.height;
				fieldRect.width = rect.width / properties.Length;
			}
			else
			{
				fieldRect.x += mainLabelRect.width;
				fieldRect.width = (rect.width - mainLabelRect.width) / properties.Length;
			}
			
			GUIContent mainLabelContent = new GUIContent();
			mainLabelContent.text = mainLabel;
			mainLabelContent.tooltip = originalGuiContent.tooltip;
			EditorGUI.LabelField(mainLabelRect, mainLabelContent);

			for (int i = 0; i < properties.Length; i++)
			{
				GUIContent label = vectors.Labels.Length > i ? new GUIContent(vectors.Labels[i]) : originalLabels[i];
				Vector2 labelSize = EditorStyles.label.CalcSize(label);
				EditorGUIUtility.labelWidth = Mathf.Max(labelSize.x + 5, 0.3f * fieldRect.width);
				
				// Use EditorGUI.PropertyField to properly handle multi-object editing
				EditorGUI.PropertyField(fieldRect, properties[i], label);
				
				fieldRect.x += fieldRect.width;
			}

			EditorGUIUtility.labelWidth = 0;
		}

		// Keep the old method for backward compatibility if needed elsewhere
		protected T[] DrawFields<T>(Rect rect, T[] vector, string mainLabel, System.Func<Rect, GUIContent, T, T> fieldDrawer, MMVectorAttribute vectors, GUIContent originalGuiContent)
		{
			T[] result = vector;

			bool shortSpace = (Screen.width < padding);

			Rect mainLabelRect = rect;
			mainLabelRect.width = EditorGUIUtility.labelWidth;
			if (shortSpace)
			{
				mainLabelRect.height *= 0.5f;
			}                

			Rect fieldRect = rect;
			if (shortSpace)
			{
				fieldRect.height *= 0.5f;
				fieldRect.y += fieldRect.height;
				fieldRect.width = rect.width / vector.Length;
			}
			else
			{
				fieldRect.x += mainLabelRect.width;
				fieldRect.width = (rect.width - mainLabelRect.width) / vector.Length;
			}
			
			GUIContent mainLabelContent = new GUIContent();
			mainLabelContent.text = mainLabel;
			mainLabelContent.tooltip = originalGuiContent.tooltip;
			EditorGUI.LabelField(mainLabelRect, mainLabelContent);

			for (int i = 0; i < vector.Length; i++)
			{
				GUIContent label = vectors.Labels.Length > i ? new GUIContent(vectors.Labels[i]) : originalLabels[i];
				Vector2 labelSize = EditorStyles.label.CalcSize(label);
				EditorGUIUtility.labelWidth = Mathf.Max(labelSize.x + 5, 0.3f * fieldRect.width);
				result[i] = fieldDrawer(fieldRect, label, vector[i]);
				fieldRect.x += fieldRect.width;
			}

			EditorGUIUtility.labelWidth = 0;
			return result;
		}
	}
}