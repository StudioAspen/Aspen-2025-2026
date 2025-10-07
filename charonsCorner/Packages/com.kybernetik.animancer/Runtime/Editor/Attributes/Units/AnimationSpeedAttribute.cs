// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer.Units
{
    /// <summary>[Editor-Conditional] Applies a different GUI for an animation speed field.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/AnimationSpeedAttribute
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class AnimationSpeedAttribute : UnitsAttribute
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimationTimeAttribute"/>.</summary>
        public AnimationSpeedAttribute()
            : base("x")
        {
            Rule = Validate.Value.IsFiniteOrNaN;
            IsOptional = true;
            DefaultValue = 1;
        }

        /************************************************************************************************************************/
    }
}

