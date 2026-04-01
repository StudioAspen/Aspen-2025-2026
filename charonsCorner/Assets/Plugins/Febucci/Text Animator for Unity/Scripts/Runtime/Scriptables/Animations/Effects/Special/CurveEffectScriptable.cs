// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
using Febucci.Parsing;
using Febucci.TextAnimatorCore.BuiltIn;
using Febucci.TextAnimatorForUnity.Effects.Core;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    class Axis
    {
        public bool isEnabled;

        public AnimationCurve weight = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -2f, 0f)
        );
        public Vector3 multiplier = new Vector3(1, 1, 1);
        [Range(0,1)] public float phaseShift;

        public bool IsValid() => isEnabled && weight != null;

        public Vector3 Sample(float pct) => weight.Evaluate((pct+phaseShift)%1f) * multiplier;
    }

    [System.Serializable]
    class ScaleAxis : Axis
    {
        public Vector3 SampleScale(float pct, float intensity)
        {
            return Vector3.LerpUnclamped(Vector3.one, Vector3.Scale(Vector3.one, multiplier), weight.Evaluate((pct+phaseShift)%1f) * intensity);
        }
    }

    public enum ColorMode
    {
        SetColor,
        Multiply
    }

    [System.Serializable]
    class GradientParam
    {
        public bool isEnabled;
        public ColorMode mode = ColorMode.SetColor;
        public Gradient gradient = new Gradient()
        {
            colorKeys = new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            }
        };
        [Range(0,1)] public float phaseShift;

        public bool IsValid() => isEnabled && gradient != null;

        public Color Sample(float pct)
        {
            return gradient.Evaluate((pct + phaseShift)%1f);
        }
    }

    [System.Serializable]
    struct CurveEffectParameters
    {
        public Axis position;
        public ScaleAxis scale;
        public Axis rotation;
        public GradientParam color;
    }

    [System.Serializable]
    struct CurveEffectState : IEffectState
    {
        bool isRotationEnabled;
        bool isMovementEnabled;
        bool isScaleEnabled;
        bool isColorEnabled;

        CurveEffectParameters current;
        readonly CurveEffectParameters defaultData;
        public CurveEffectState(CurveEffectParameters defaultData)
        {
            this.defaultData = defaultData;
            this.current = defaultData;

            isColorEnabled = defaultData.color != null && defaultData.color.IsValid();
            isMovementEnabled = defaultData.position != null && defaultData.position.IsValid();
            isRotationEnabled = defaultData.rotation != null && defaultData.rotation.IsValid();
            isScaleEnabled = defaultData.scale != null && defaultData.scale.IsValid();
        }

        public void UpdateParameters(RegionParameters parameters)
        {
            // TODO parameters
        }

        public void Apply(ref CharacterData character, in ManagedEffectContext context)
        {
            float progress = context.isInsideBehavior
                ? context.progression01
                : context.intensity;

            float intensity = context.isInsideBehavior ? context.intensity : 1;
            float modifier = context.isInsideBehavior ? 1 : context.progressionRange; // opposite dirs

            if (isRotationEnabled)
                character.RotateDegrees(current.rotation.Sample(progress).z * intensity * modifier, context.isUpPositive);

            if (isMovementEnabled)
                character.MovePosition(current.position.Sample(progress) * intensity * modifier, context.isUpPositive);

            if (isScaleEnabled)
                character.Scale(current.scale.SampleScale(progress, intensity)*modifier);

            // PSA no offset on gradient!
            // this is expected, otherwise it'd be a weird effect
            if (isColorEnabled)
            {
                switch (current.color.mode)
                {
                    case ColorMode.Multiply:
                        for (int i = 0; i < CharacterData.VERTICES_PER_CHAR; i++)
                        {
                            character.current.colors[i] =
                                Numbers.Color32.LerpUnclamped(character.current.colors[i],
                                    (Color32)character.current.colors[i] *
                                    current.color.Sample(progress),
                                    intensity);
                        }
                        break;
                    case ColorMode.SetColor:
                        character.LerpColor(current.color.Sample(progress), intensity);
                        break;
                }
            }
        }
    }

    [UnityEngine.Scripting.Preserve]
    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.EFFECT_STATES_SPECIAL+"Curve", fileName = "Curve Effect")]
    sealed class CurveEffectScriptable : ManagedEffectScriptableBase
    {
        [SerializeField] string tagID;
        public override string TagID
        {
            get => tagID;
            set => tagID = value;
        }

        [System.Serializable]
        class CurveEffectContent : IEffectContent
        {
            [SerializeField] CurveEffectParameters state;
            [SerializeField] DefaultPhaseParams phase;
            [SerializeField] EffectPlaybackScriptableBase playback;
            [SerializeField] EffectCurveScriptableBase curve;

            public IEffectPhase CreatePhase() => new DefaultPhase(phase.charOffset, phase.wordOffset, phase.speed);
            public IEffectState CreateState() => new CurveEffectState(state);

            public IEffectPlayback Playback => playback;


            // defaults to linear curve to have inspector curves make sense (sampling 0->1 correctly)
            readonly static TextAnimatorCore.BuiltIn.LinearCurve FallbackCurve = new TextAnimatorCore.BuiltIn.LinearCurve();
            public IEffectCurve StateCurve => curve != null ? curve : FallbackCurve;
        }

        [SerializeField] EffectPresetSettings settings;
        [SerializeField] CurveEffectContent appearance;
        [SerializeField] CurveEffectContent persistent;
        [SerializeField] CurveEffectContent disappearance;

        public override IEffectContent Appearance => appearance;
        public override IEffectContent Disappearance => disappearance;
        public override IEffectContent Persistent => persistent;
        public override EffectPresetSettings Settings => settings;

        #if UNITY_EDITOR
        void OnValidate()
        {
            NotifyValueChanged();
        }
        #endif
    }
}