// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>[Editor-Conditional] Specifies the default value of a field and a secondary fallback.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/DefaultValueAttribute
    [AttributeUsage(AttributeTargets.Field)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public class DefaultValueAttribute : Attribute
    {
        /************************************************************************************************************************/

        /// <summary>The main default value.</summary>
        public virtual object Primary { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>The fallback value to use if the target value was already equal to the <see cref="Primary"/>.</summary>
        public virtual object Secondary { get; protected set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DefaultValueAttribute"/>.</summary>
        public DefaultValueAttribute(object primary, object secondary = null)
        {
            Primary = primary;
            Secondary = secondary;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="DefaultValueAttribute"/>.</summary>
        protected DefaultValueAttribute() { }

        /************************************************************************************************************************/
    }
}

