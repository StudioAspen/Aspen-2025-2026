using UnityEngine;
using UnityEngine.Splines;

namespace CharonsCorner.LevelEditor
{
	public static class SplineDataExtensions
	{
		public static void SetFloatAtT(this SplineData<float> data, float t, float value)
		{
			float epsilon = 1e-4f;
			
			// If there’s already a datapoint at (approximately) t, replace it.
			int i = 0;
			foreach (var dp in data) // DataPoint<T> has (Index, Value)
			{
				if (Mathf.Abs(dp.Index - t) <= epsilon)
				{
					data.SetDataPoint(i, new DataPoint<float>(t, value));
					return;
				}
				i++;
			}

			// Otherwise add a new point at t
			data.Add(t, value);
		}
	}
}