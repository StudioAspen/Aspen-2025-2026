// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine.Scripting.APIUpdating;

namespace Animancer.Editor
{
    /// <summary>[Editor-Conditional]
    /// A <see cref="MovedFromAttribute"/> which indicates that a type may have been previously
    /// defined in the pre-compiled Animancer Lite DLL in an earlier version of Animancer.
    /// </summary>
    /// <remarks>
    /// This allows <see cref="UnityEngine.SerializeReference"/> fields of the attributed type
    /// to retain their values when upgrading from Animancer Lite to Animancer Pro.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/InternalSerializableTypeAttribute
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class InternalSerializableTypeAttribute : MovedFromAttribute
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="InternalSerializableTypeAttribute"/>.</summary>
        public InternalSerializableTypeAttribute()
            : base(true, sourceAssembly: Strings.LiteAssemblyName)
        {
        }

        /************************************************************************************************************************/
    }
}

