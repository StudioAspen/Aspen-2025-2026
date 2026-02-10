// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [UnityEngine.Scripting.Preserve]
    [System.Serializable]
    [CreateAssetMenu(fileName = "Custom Curve", menuName = ScriptablePaths.CURVE_PATH+"Custom")]
    public sealed class CustomCurveScriptable : EffectCurveScriptableBase
    {
        [SerializeField] AnimationCurve curve01 = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(1f, 1f, 2f, 0f)
        );

        [SerializeField] AnimationCurve curveRange = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3f),
            new Keyframe(0.333f, 1f, 0f, 0f),
            new Keyframe(0.666f, -1f, 0f, 0f),
            new Keyframe(1f, 0f, 3f, 0f)
        ) { preWrapMode = WrapMode.Loop, postWrapMode = WrapMode.Loop };

        public override float Evaluate01(float time) => curve01.Evaluate(time);
        public override float EvaluateRange(float time) => curveRange.Evaluate(time);

        public override void Initialize()
        {
            if (curve01 == null)
                throw new NullReferenceException($"Assigned curve 01 is null: {name}");

            if (curveRange == null)
                throw new NullReferenceException($"Assigned curve range is null: {name}");
        }
        public override int BakeResolution => 360;
    }
}