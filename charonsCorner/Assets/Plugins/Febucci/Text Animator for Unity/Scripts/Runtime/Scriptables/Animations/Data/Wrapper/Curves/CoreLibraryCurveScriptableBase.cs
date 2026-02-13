// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using Febucci.TextAnimatorCore;

using UnityEngine;
namespace Febucci.TextAnimatorForUnity.Effects
{
    /// <summary>
    /// Wrapper class for animating letters in Text Animator via Core Library e ffects
    /// </summary>
    [System.Serializable]
    public abstract class CoreLibraryCurveScriptableBase<TEffectCurve> : EffectCurveScriptableBase where TEffectCurve : IEffectCurve, new()
    {
        [SerializeField] TEffectCurve curve;

        //--- INITIALIZATION ---
        bool initialized = false;
        public override sealed void Initialize()
        {
            if(initialized) return;
            initialized = true;

            curve ??= new TEffectCurve();

            if (curve == null)
                throw new Exception($"Unable to instantiate effect of type: {typeof(TEffectCurve)}");

            OnInitialize();
        }

        protected virtual void OnInitialize() { }

        public override float Evaluate01(float time) => curve.Evaluate01(time);
        public override float EvaluateRange(float time) => curve.EvaluateRange(time);

        public override int BakeResolution => curve.BakeResolution;

        void OnEnable()
        {
            //resets for enter playmode settings
            initialized = false;
        }
    }
}