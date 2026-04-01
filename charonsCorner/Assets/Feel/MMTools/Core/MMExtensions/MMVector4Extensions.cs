using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Vector4 Extensions
	/// </summary>
	public static class MMVector4Extensions
	{
		/// <summary>
		/// Returns the sum of all components of a vector4
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static float SumComponents(this Vector4 vector)
		{
			return vector.x + vector.y + vector.z + vector.w;
		}
		
		/// <summary>
		/// Sets the x value of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Vector4 MMSetX(this Vector4 vector, float newValue)
		{
			vector.x = newValue;
			return vector;
		}

		/// <summary>
		/// Sets the y value of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Vector4 MMSetY(this Vector4 vector, float newValue)
		{
			vector.y = newValue;
			return vector;
		}

		/// <summary>
		/// Sets the z value of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Vector4 MMSetZ(this Vector4 vector, float newValue)
		{
			vector.z = newValue;
			return vector;
		}

		/// <summary>
		/// Sets the z value of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Vector4 MMSetW(this Vector4 vector, float newValue)
		{
			vector.w = newValue;
			return vector;
		}

		/// <summary>
		/// Inverts a vector
		/// </summary>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static Vector4 MMInvert(this Vector4 newValue)
		{
			return new Vector4
			(
				1.0f / newValue.x,
				1.0f / newValue.y,
				1.0f / newValue.z,
				1.0f / newValue.w
			);
		}

		/// <summary>
		/// Projects a vector on another
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="projectedVector"></param>
		/// <returns></returns>
		public static Vector4 MMProject(this Vector4 vector, Vector4 projectedVector)
		{
			float _dot = Vector4.Dot(vector, projectedVector);
			return _dot * projectedVector;
		}

		/// <summary>
		/// Rejects a vector on another
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="rejectedVector"></param>
		/// <returns></returns>
		public static Vector4 MMReject(this Vector4 vector, Vector4 rejectedVector)
		{
			return vector - vector.MMProject(rejectedVector);
		}

		/// <summary>
		/// Rounds all components of a vector
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public static Vector4 MMRound(this Vector4 vector)
		{
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			vector.z = Mathf.Round(vector.z);
			vector.w = Mathf.Round(vector.w);
			return vector;
		}
	}
}