using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public struct IntRange
    {
        public int Min;
        public int Max;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Clamps the value between the minimum and maximum values of the range.
        /// Returns the clamped value.
        /// </summary>
        public int Clamp(int value) => Mathf.Clamp(value, Min, Max);
        /// <summary>
        /// Clamps the value between the minimum and maximum values of the range
        /// and returns a value between 0 and 1, where 0 corresponds to Min and 1 corresponds to Max.
        /// </summary>
        public float Clamp01(int value) => Mathf.Clamp(InverseLerp(value), Min, Max);
        /// <summary>
        /// Checks if the value is within the range defined by Min and Max.
        /// </summary>
        public bool Contains(int value) => value >= Min && value <= Max;
        /// <summary>
        /// Linearly interpolates between Min and Max based on the parameter t.
        /// </summary>
        public int Lerp(float t) => (int)Mathf.Lerp(Min, Max, t);
        /// <summary>
        /// Gets the parameter t that corresponds to the given value within the range
        /// such that Min corresponds to 0 and Max corresponds to 1.
        /// </summary>
        public float InverseLerp(int value) => Mathf.InverseLerp(Min, Max, value);
        /// <summary>
        /// Gets a random value within the range defined by Min and Max.
        /// </summary>
        public int RandomValue() => UnityEngine.Random.Range(Min, Max);
    }
}
