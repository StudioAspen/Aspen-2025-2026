using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{	
	[CustomPropertyDrawer(typeof(MMLayerAttribute))]
	class LayerAttributeEditor : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			property.intValue = EditorGUI.LayerField(position, label,  property.intValue);
		}
	}
}
