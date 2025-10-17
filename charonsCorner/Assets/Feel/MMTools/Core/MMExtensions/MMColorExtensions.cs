using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Color extensions
	/// </summary>
	public static class MMColorExtensions
	{
		/// <summary>
		/// Adds all parts of the color and returns a float
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float MMSum(this Color color)
		{
			return color.r + color.g + color.b + color.a;
		}

		/// <summary>
		/// Returns a mean value between r, g and b
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float MMMeanRGB(this Color color)
		{
			return (color.r + color.g + color.b) / 3f;
		}

		/// <summary>
		/// Computes the color's luminance value
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		public static float MMLuminance(this Color color)
		{
			return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b;
		}
		
		public static Color MMLighten(this Color color, float amount)
		{
			amount = Mathf.Clamp01(amount);
			color.r = color.r / amount;
			color.g = color.g / amount;
			color.b = color.b / amount;
			return color;
		}
		
		public static Color MMDarken(this Color color, float amount)
		{
			amount = Mathf.Clamp01(amount);
			color.r = color.r * (1 - amount);
			color.g = color.g * (1 - amount);
			color.b = color.b * (1 - amount);
			return color;
		}
		
		public static Color32 MMDarken(this Color32 color, float amount)
		{
			amount = 1 - Mathf.Clamp01(amount);
			color.r = (byte)(color.r * amount);
			color.g = (byte)(color.g * amount);
			color.b = (byte)(color.b * amount);
			return color;
		}
		
		public static Color MMAlpha(this Color color, float newAlpha)
		{
			newAlpha = Mathf.Clamp01(newAlpha); 
			color.a = newAlpha;
			return color;
		}
	}
}