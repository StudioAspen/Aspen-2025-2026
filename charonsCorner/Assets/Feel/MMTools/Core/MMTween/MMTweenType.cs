using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MoreMountains.Tools
{
	public enum MMTweenDefinitionTypes { MMTween, AnimationCurve }

	[Serializable]
	public class MMTweenType
	{
		public static MMTweenType DefaultEaseInCubic { get; } = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		public MMTweenDefinitionTypes MMTweenDefinitionType = MMTweenDefinitionTypes.MMTween;
		public MMTween.MMTweenCurve MMTweenCurve = MMTween.MMTweenCurve.EaseInCubic;
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
		public bool Initialized = false;
		
		public string ConditionPropertyName = "";
		public string EnumConditionPropertyName = "";
		public bool[] EnumConditions = new bool[32];

		public MMTweenType(MMTween.MMTweenCurve newCurve, string conditionPropertyName = "", string enumConditionPropertyName = "", params int[] enumConditionValues)
		{
			MMTweenCurve = newCurve;
			MMTweenDefinitionType = MMTweenDefinitionTypes.MMTween;
			ConditionPropertyName = conditionPropertyName;
			EnumConditionPropertyName = enumConditionPropertyName;
			for (int i = 0; i < enumConditionValues.Length; i++)
			{
				EnumConditions[enumConditionValues[i]] = true;
			}
		}
		public MMTweenType(AnimationCurve newCurve, string conditionPropertyName = "", string enumConditionPropertyName = "", params int[] enumConditionValues)
		{
			Curve = newCurve;
			MMTweenDefinitionType = MMTweenDefinitionTypes.AnimationCurve;
			ConditionPropertyName = conditionPropertyName;
			EnumConditionPropertyName = enumConditionPropertyName;
			for (int i = 0; i < enumConditionValues.Length; i++)
			{
				EnumConditions[enumConditionValues[i]] = true;
			}
		}

		public float Evaluate(float t)
		{
			return MMTween.Evaluate(t, this);
		}
	}
}