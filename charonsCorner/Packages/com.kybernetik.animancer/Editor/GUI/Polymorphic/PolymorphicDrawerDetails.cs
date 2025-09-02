// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using Animancer.Editor;
using System;
using System.Collections.Generic;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// An assembly attribute for configuring how the <see cref="PolymorphicDrawer"/>
    /// displays a particular type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class PolymorphicDrawerDetails : Attribute
    {
        /************************************************************************************************************************/

        /// <summary>A default instance.</summary>
        public static readonly PolymorphicDrawerDetails
            Default = new(null);

        /************************************************************************************************************************/

        /// <summary>The <see cref="System.Type"/> this attribute applies to.</summary>
        public readonly Type Type;

        /// <summary>Creates a new <see cref="PolymorphicDrawerDetails"/>.</summary>
        public PolymorphicDrawerDetails(Type type)
            => Type = type;

        /************************************************************************************************************************/

        /// <summary>
        /// Should the label and <see cref="TypeSelectionButton"/>
        /// be drawn on a separate line before the field's regular GUI?
        /// </summary>
        public bool SeparateHeader { get; set; }

        /************************************************************************************************************************/

        private static readonly Dictionary<Type, PolymorphicDrawerDetails>
            TypeToDetails = new();

        /// <summary>Gathers all instances of this attribute in all currently loaded assemblies.</summary>
        static PolymorphicDrawerDetails()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
            {
                var assembly = assemblies[iAssembly];
                if (!assembly.IsDefined(typeof(PolymorphicDrawerDetails), false))
                    continue;

                var attributes = assemblies[iAssembly].GetCustomAttributes(typeof(PolymorphicDrawerDetails), false);
                for (int iAttribute = 0; iAttribute < attributes.Length; iAttribute++)
                {
                    var attribute = (PolymorphicDrawerDetails)attributes[iAttribute];
                    TypeToDetails.Add(attribute.Type, attribute);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the <see cref="PolymorphicDrawerDetails"/> associated with the `type` or any of its base types.
        /// Returns <c>null</c> if none of them have any details.
        /// </summary>
        public static PolymorphicDrawerDetails Get(Type type)
        {
            if (TypeToDetails.TryGetValue(type, out var details))
                return details;

            if (type.BaseType != null)
                details = Get(type.BaseType);
            else
                details = Default;

            TypeToDetails.Add(type, details);
            return details;
        }

        /// <summary>
        /// Returns the <see cref="PolymorphicDrawerDetails"/> associated with the `obj` or any of its base types.
        /// Returns <c>null</c> if none of them have any details.
        /// </summary>
        public static PolymorphicDrawerDetails Get(object obj)
            => obj == null
            ? Default
            : Get(obj.GetType());

        /************************************************************************************************************************/
    }
}

#endif
