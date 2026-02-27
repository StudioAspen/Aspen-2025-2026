using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[Serializable]
	public struct MMInterval<T> where T : struct, IComparable
	{
		public enum MMIntervalType { Inclusive, Exclusive }
		
		/// the lower bound of this interval
		[Tooltip("the lower bound of this interval")]
		public T LowerBound;
		/// the upper bound of this interval
		[Tooltip("the upper bound of this interval")]
		public T UpperBound;
		/// whether to include or exclude the lower bound in the interval
		[Tooltip("whether to include or exclude the lower bound in the interval")]
		public MMIntervalType LowerBoundIntervalType;
		/// whether to include or exclude the upper bound in the interval
		[Tooltip("whether to include or exclude the upper bound in the interval")]
		public MMIntervalType UpperBoundIntervalType;
		
		/// <summary>
		/// Creates an interval with the specified bounds 
		/// </summary>
		/// <param name="lowerBound"></param>
		/// <param name="upperBound"></param>
		/// <param name="lowerboundIntervalType"></param>
		/// <param name="upperboundIntervalType"></param>
		public MMInterval(T lowerBound, T upperBound, MMIntervalType lowerboundIntervalType = MMIntervalType.Inclusive, MMIntervalType upperboundIntervalType = MMIntervalType.Inclusive) : this()
		{
			T a = lowerBound;
			T b = upperBound;
			int comparison = a.CompareTo(b);

			if (comparison > 0)
			{
				a = upperBound;
				b = lowerBound;
			}

			LowerBound = a;
			UpperBound = b;
			LowerBoundIntervalType = lowerboundIntervalType;
			UpperBoundIntervalType = upperboundIntervalType;
		}
		
		/// <summary>
		/// Returns true if the interval contains the specified value 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Contains(T value)
		{
			bool lowerBoundCheck = LowerBoundIntervalType == MMIntervalType.Exclusive ? LowerBound.CompareTo(value) < 0 : LowerBound.CompareTo(value) <= 0;
			bool upperBoundCheck = UpperBoundIntervalType == MMIntervalType.Exclusive ? UpperBound.CompareTo(value) > 0 : UpperBound.CompareTo(value) >= 0;
			
			return lowerBoundCheck && upperBoundCheck;
		}
	}
}

