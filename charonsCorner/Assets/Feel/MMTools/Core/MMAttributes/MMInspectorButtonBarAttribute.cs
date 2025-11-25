using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEditor;
#endif
using System.Reflection;
using UnityEngine.UIElements;

namespace MoreMountains.Tools
{	
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class MMInspectorButtonBarAttribute : PropertyAttribute
	{
		public string[] Labels { get; set; }
		public string[] Methods{ get; set; }
		public bool[] OnlyWhenPlaying{ get; set; }
		public string[] UssClass{ get; set; }


		public MMInspectorButtonBarAttribute(string[] labels, string[] methods, bool[] onlyWhenPlaying, string[] ussClass)
		{
			this.Labels = labels;
			this.Methods = methods;
			this.OnlyWhenPlaying = onlyWhenPlaying;
			this.UssClass = ussClass;
		}
	}

	#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(MMInspectorButtonBarAttribute))]
	public class MMInspectorButtonBarPropertyDrawer : PropertyDrawer
	{
		private MethodInfo[] _eventMethodInfos = null;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			MMInspectorButtonBarAttribute inspectorButtonBarAttribute = (MMInspectorButtonBarAttribute)attribute;
			System.Type eventOwnerType = property.serializedObject.targetObject.GetType();

			// add our root
			var root = new VisualElement();
			
			// add toolbar
			Toolbar moveToControls = new Toolbar();
			moveToControls.AddToClassList("mm-toolbar");

			if (_eventMethodInfos == null)
			{
				_eventMethodInfos = new MethodInfo[inspectorButtonBarAttribute.Methods.Length];
			}
			
			// add each button
			for (var i = 0; i < inspectorButtonBarAttribute.Labels.Length; i++)
			{
				var newButton = new ToolbarButton();
				newButton.text = inspectorButtonBarAttribute.Labels[i];
				newButton.style.flexGrow = 1;
				
				if (inspectorButtonBarAttribute.UssClass[i] != "")
				{
					newButton.AddToClassList(inspectorButtonBarAttribute.UssClass[i]);
				}
				
				if (_eventMethodInfos[i] == null)
				{
					_eventMethodInfos[i] = eventOwnerType.GetMethod(inspectorButtonBarAttribute.Methods[i], BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				if (_eventMethodInfos[i] != null)
				{
					var i1 = i;
					newButton.clicked += () => _eventMethodInfos[i1].Invoke(property.serializedObject.targetObject, null);
				}
				else
				{
					Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", inspectorButtonBarAttribute.Methods[i], eventOwnerType));
				}
				
				if (inspectorButtonBarAttribute.OnlyWhenPlaying[i] && !Application.isPlaying)
				{
					newButton.SetEnabled(false);
				}
				
				moveToControls.Add(newButton);
			}
			root.Add(moveToControls);
			
			return root;
			/*

			if (GUI.Button(buttonRect, inspectorButtonBarAttribute.MethodName))
			{
				System.Type eventOwnerType = prop.serializedObject.targetObject.GetType();
				string eventName = inspectorButtonBarAttribute.MethodName;

				if (_eventMethodInfo == null)
				{
					_eventMethodInfo = eventOwnerType.GetMethod(eventName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}

				if (_eventMethodInfo != null)
				{
					_eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
				}
				else
				{
					Debug.LogWarning(string.Format("InspectorButton: Unable to find method {0} in {1}", eventName, eventOwnerType));
				}
			}*/
		}
	}
	#endif
}