using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace MoreMountains.Tools
{	
	public static class MMUIToolkit 
	{
		#if UNITY_EDITOR

		public static PropertyField CreateAndBindPropertyField(string propertyName, SerializedObject serializedObject, VisualElement newParent)
		{
			PropertyField propertyField = new PropertyField(serializedObject.FindProperty(propertyName));
			propertyField.Bind(serializedObject);  
			newParent.Add(propertyField);
			return propertyField;
		}
		
		
		#endif
	    
	        
    }
}
