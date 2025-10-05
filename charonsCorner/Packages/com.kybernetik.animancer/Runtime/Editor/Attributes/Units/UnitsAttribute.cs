// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;

namespace Animancer.Units
{
    /// <summary>[Editor-Conditional]
    /// Causes a float field to display a suffix to indicate what kind of units the value represents as well as
    /// displaying it as several different fields which convert the value between different units.
    /// </summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/UnitsAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public class UnitsAttribute : PropertyAttribute
    {
        /************************************************************************************************************************/

        /// <summary>The multipliers to convert from the field's actual value to each unit type.</summary>
        /// <remarks><c>valueInUnitX = valueInBaseUnits * Multipliers[x];</c></remarks>
        public readonly float[] Multipliers;

        /// <summary>The unit suffix to display at the end of the value in each field.</summary>
        public readonly string[] Suffixes;

        /// <summary>The index of the multiplier where the field stores its actual value.</summary>
        /// <remarks>The multiplier at this index must always be 1.</remarks>
        public readonly int UnitIndex;

        /************************************************************************************************************************/

        /// <summary>The validation rule applied to the value.</summary>
        public Validate.Value Rule { get; set; }

        /// <summary>Should the field have a toggle to set its value to <see cref="float.NaN"/>?</summary>
        public bool IsOptional { get; set; }

        /// <summary>The value to display if the actual value is <see cref="float.NaN"/>.</summary>
        public float DefaultValue { get; set; }

        /// <summary>Optional text to display instead of the regular fields when the value is <see cref="float.NaN"/>.</summary>
        public string DisabledText { get; set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="UnitsAttribute"/>.</summary>
        protected UnitsAttribute() { }

        /// <summary>Creates a new <see cref="UnitsAttribute"/>.</summary>
        public UnitsAttribute(string suffix)
        {
            Multipliers = new float[] { 1 };
            Suffixes = new string[] { suffix };
        }

        /// <summary>Creates a new <see cref="UnitsAttribute"/>.</summary>
        public UnitsAttribute(float[] multipliers, string[] suffixes, int unitIndex = 0)
        {
            Multipliers = multipliers;
            Suffixes = suffixes;
            UnitIndex = unitIndex;

            Debug.Assert(multipliers.Length == suffixes.Length,
                $"[Units] The {nameof(multipliers)} and {nameof(suffixes)} arrays have different lengths.");
            Debug.Assert((uint)UnitIndex < (uint)multipliers.Length,
                $"[Units] The {nameof(unitIndex)} is outside the {nameof(multipliers)} array.");
        }

        /************************************************************************************************************************/
    }
}

