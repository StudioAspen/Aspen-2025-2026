using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[CustomPropertyDrawer(typeof(MMSpringDebug))]
	public class MMSpringDebugPropertyDrawer : PropertyDrawer
	{
		protected Color _backgroundBarColor = new Color(0f, 0f, 0f, 0.3f);
		protected Color _frontBarColor = MMColors.Yellow;
		
		protected SerializedProperty _currentValue;
		protected SerializedProperty _targetValue;

		protected Rect _rect;
		protected float _lastTarget;
		protected float _max;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			_currentValue = property.FindPropertyRelative("CurrentValue");
			_targetValue = property.FindPropertyRelative("TargetValue");
			
			EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginDisabledGroup(true);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			_rect.x = position.x;
			_rect.y = position.y;
			_rect.width = position.width;
			_rect.height = position.height;
			EditorGUI.DrawRect(_rect, _backgroundBarColor);

			if (Application.isPlaying)
			{
				float current = _currentValue.floatValue;
				float target = _targetValue.floatValue;
				float normalizedValue = 0f;
				float diff = target - current;
				if (Mathf.Abs(diff) > Mathf.Abs(_max))
				{
					_max = diff;
				}

				if (_lastTarget != target)
				{
					_max = diff;
				}
				
				normalizedValue = MMMaths.Remap(diff, -_max, _max, -1f, 1f);	

				float newWidth = MMMaths.Remap(normalizedValue, -1f, 1f, -position.width/2f, position.width/2f);
				_rect.x = position.x + (position.width/2f);
				_rect.y = position.y;
				_rect.width = newWidth;
				_rect.height = position.height;
				EditorGUI.DrawRect(_rect, _frontBarColor);
				
				_lastTarget = target;
			}

			EditorGUI.indentLevel = indent;
			EditorGUI.EndDisabledGroup();
			EditorGUI.EndProperty();

		}
	}	
}
