using Lofelt.NiceVibrations;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// A custom drawer for haptic data used by the NV Clip feedback
	/// </summary>
	[CustomPropertyDrawer(typeof(NVHapticData))]
	public class NVHapticDataDrawer : PropertyDrawer
	{
		/// <summary>
		/// Property height computation
		/// </summary>
		/// <param name="property"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			NVHapticData data = property.boxedValue as NVHapticData;

			if (data.Clip == null)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else
			{
				return EditorGUIUtility.singleLineHeight * 14;
			}
		}

		/// <summary>
		/// Drawer
		/// </summary>
		/// <param name="position"></param>
		/// <param name="property"></param>
		/// <param name="label"></param>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			SerializedProperty rumbleData = property.FindPropertyRelative("RumbleData");
			SerializedProperty totalDurationProp = rumbleData.FindPropertyRelative("totalDurationMs");
			NVHapticData data = property.boxedValue as NVHapticData;

			if (data.Clip == null)
			{
				return;
			}

			EditorGUI.LabelField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
				label);

			float lineHeight = EditorGUIUtility.singleLineHeight;
			float y = position.y + lineHeight;

			EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), totalDurationProp);
			y += lineHeight + 2;

			// Generate curves

			int[] amplitudeDurations = new int[data.AmplitudePoints.Count];
			float[] amplitudeValues = new float[data.AmplitudePoints.Count];
			float[] frequencyValues = new float[data.AmplitudePoints.Count];
			for (int i = 0; i < data.AmplitudePoints.Count; i++)
			{
				var point = data.AmplitudePoints[i];
				amplitudeDurations[i] = (int)(data.AmplitudePoints[i].time * 1000f);
				amplitudeValues[i] = data.AmplitudePoints[i].emphasis.amplitude;
				frequencyValues[i] = data.FrequencyPoints[i].frequency;
			}

			AnimationCurve amplitudeCurve = GenerateCurve(amplitudeDurations, amplitudeValues);
			AnimationCurve emphasisCurve = GenerateCurve(amplitudeDurations, frequencyValues);
			// Create rect for combined curve
			Rect amplitudeCurveRect = new Rect(position.x, y, position.width, lineHeight * 5);
			// Draw background
			EditorGUI.DrawRect(amplitudeCurveRect, new Color(0.15f, 0.15f, 0.15f));
			// Draw both curves manually
			Handles.BeginGUI();
			DrawCurveInRect(amplitudeCurve, amplitudeCurveRect, Color.yellow, data.SampleCount);
			DrawCurveInRect(emphasisCurve, amplitudeCurveRect, Color.red, data.SampleCount);
			Handles.EndGUI();

			EditorGUI.LabelField(new Rect(position.x, y + amplitudeCurveRect.height, position.width, lineHeight),
				"Yellow = Amplitude  |  Red = Frequency");

			y += lineHeight * 6;

			// Generate curves
			AnimationCurve lowCurve = GenerateCurve(amplitudeDurations, data.RumbleData.lowFrequencyMotorSpeeds);
			AnimationCurve highCurve = GenerateCurve(amplitudeDurations, data.RumbleData.highFrequencyMotorSpeeds);
			// Create rect for combined curve
			Rect curveRect = new Rect(position.x, y, position.width, lineHeight * 5);
			// Draw background
			EditorGUI.DrawRect(curveRect, new Color(0.15f, 0.15f, 0.15f));
			// Draw both curves manually
			Handles.BeginGUI();
			DrawCurveInRect(lowCurve, curveRect, Color.green, data.SampleCount);
			DrawCurveInRect(highCurve, curveRect, Color.cyan, data.SampleCount);
			Handles.EndGUI();

			EditorGUI.LabelField(new Rect(position.x, y + curveRect.height, position.width, lineHeight),
				"Cyan = Amplitude  |  Green = Frequency");
		}

		/// <summary>
		/// Curve drawing
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="rect"></param>
		/// <param name="color"></param>
		/// <param name="sampleCount"></param>
		private void DrawCurveInRect(AnimationCurve curve, Rect rect, Color color, int sampleCount)
		{
			if (curve.length < 2)
				return;

			Handles.color = color;
			Vector3[] points = new Vector3[sampleCount];

			float startTime = curve.keys[0].time;
			float endTime = curve.keys[curve.length - 1].time;
			float timeRange = endTime - startTime;

			for (int i = 0; i < points.Length; i++)
			{
				float t = Mathf.Lerp(startTime, endTime, i / (float)(points.Length - 1));
				float val = curve.Evaluate(t);

				float x = Mathf.Lerp(rect.x, rect.xMax, (t - startTime) / timeRange);
				float y = Mathf.Lerp(rect.yMax, rect.y, val); 

				points[i] = new Vector3(x, y, 0);
			}

			Handles.DrawAAPolyLine(2f, points);
		}

		/// <summary>
		/// Curve generation
		/// </summary>
		/// <param name="durationsProperty"></param>
		/// <param name="valueProperty"></param>
		/// <returns></returns>
		private AnimationCurve GenerateCurve(int[] durationsProperty, float[] valueProperty)
		{
			AnimationCurve curve = new AnimationCurve();

			if (durationsProperty == null || valueProperty == null || durationsProperty.Length != valueProperty.Length)
				return curve;

			float time = 0f;
			for (int i = 0; i < valueProperty.Length; i++)
			{
				int duration = durationsProperty[i];
				float speed = valueProperty[i];

				curve.AddKey(time, speed);
				time += duration / 1000f;
			}

			return curve;
		}
	}
}