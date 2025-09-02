// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer.Units
{
    /// <summary>[Editor-Conditional]
    /// Causes a float field to display using 3 fields: Normalized, Seconds, and Frames.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/transitions#time-fields">
    /// Time Fields</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/AnimationTimeAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class AnimationTimeAttribute : UnitsAttribute
    {
        /************************************************************************************************************************/

        /// <summary>A unit of measurement used by the <see cref="AnimationTimeAttribute"/>.</summary>
        public enum Units
        {
            /// <summary>A value of 1 represents the end of the animation.</summary>
            Normalized = 0,

            /// <summary>A value of 1 represents 1 second.</summary>
            Seconds = 1,

            /// <summary>A value of 1 represents 1 frame.</summary>
            Frames = 2,
        }

        /// <summary>An explanation of the suffixes used in fields drawn by this attribute.</summary>
        public const string Tooltip = "x = Normalized, s = Seconds, f = Frames";

        /// <summary>The <see cref="UnitsAttribute.Multipliers"/> used by instances of this attribute.</summary>
        private static new readonly float[] Multipliers = new float[3];// Calculated immediately before each use.

        /// <summary>The <see cref="UnitsAttribute.Suffixes"/> used by instances of this attribute.</summary>
        private static new readonly string[] Suffixes = new string[3] { "x", "s", "f" };

        /************************************************************************************************************************/

        /// <summary>Cretes a new <see cref="AnimationTimeAttribute"/>.</summary>
        public AnimationTimeAttribute(Units units)
            : base(Multipliers, Suffixes, (int)units)
        { }

        /************************************************************************************************************************/
    }
}

