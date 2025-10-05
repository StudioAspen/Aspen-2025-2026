// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

namespace Animancer.Units
{
    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Angle measured in <c>degrees</c> (<c>º</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/DegreesAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DegreesAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="DegreesAttribute"/>.</summary>
        public DegreesAttribute() : base(" º") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Rotational speed measured in <c>degrees per second</c> (<c>º/s</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/DegreesPerSecondAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class DegreesPerSecondAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="DegreesPerSecondAttribute"/>.</summary>
        public DegreesPerSecondAttribute() : base(" º/s") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Distance measured in <c>meters</c> (<c>m</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/MetersAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="MetersAttribute"/>.</summary>
        public MetersAttribute() : base(" m") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Speed measured in <c>meters per second</c> (<c>m/s</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/MetersPerSecondAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersPerSecondAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="MetersPerSecondAttribute"/>.</summary>
        public MetersPerSecondAttribute() : base(" m/s") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Acceleration measured in <c>meters per second per second</c> (<c>m/s²</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/MetersPerSecondPerSecondAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MetersPerSecondPerSecondAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="MetersPerSecondPerSecondAttribute"/>.</summary>
        public MetersPerSecondPerSecondAttribute() : base(" m/s\xB2") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] A multiplier displayed with an <c>x</c> suffix.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/MultiplierAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class MultiplierAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="MultiplierAttribute"/>.</summary>
        public MultiplierAttribute() : base(" x") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] Time measured in <c>seconds</c> (<c>s</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/SecondsAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class SecondsAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="SecondsAttribute"/>.</summary>
        public SecondsAttribute() : base(" s") { }
    }

    /************************************************************************************************************************/

    /// <summary>[Editor-Conditional] A value measured <c>per second</c> (<c>/s</c>).</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/other/units">
    /// Units Attribute</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Units/PerSecondAttribute
    /// 
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class PerSecondAttribute : UnitsAttribute
    {
        /// <summary>Creates a new <see cref="PerSecondAttribute"/>.</summary>
        public PerSecondAttribute() : base(" /s") { }
    }

    /************************************************************************************************************************/
}

