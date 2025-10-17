using UnityEngine;

namespace MoreMountains.Tools
{
	public static class MMAnimationCurves
	{
		public static AnimationCurve LerpAnimationCurves(AnimationCurve a, AnimationCurve b, float t, int samplePoints = 20)
		{
			AnimationCurve result = new AnimationCurve();

			float startTime = Mathf.Min(a.keys[0].time, b.keys[0].time);
			float endTime = Mathf.Max(a.keys[a.length - 1].time, b.keys[b.length - 1].time);

			Keyframe[] keyframes = new Keyframe[samplePoints + 1];

			for (int i = 0; i <= samplePoints; i++)
			{
				float time = Mathf.Lerp(startTime, endTime, i / (float)samplePoints);
				float valueA = a.Evaluate(time);
				float valueB = b.Evaluate(time);

				float lerpedValue = Mathf.Lerp(valueA, valueB, t);
				keyframes[i] = new Keyframe(time, lerpedValue);
			}

			result.keys = keyframes;
			for (int i = 0; i < result.keys.Length; i++)
			{
				result.SmoothTangents(i, 0f);
			}

			return result;
		}
	}
}
