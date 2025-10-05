// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using UnityEngine;

namespace Animancer
{
    /************************************************************************************************************************/

    /// <summary>
    /// An object that will be drawn by a <see cref="Editor.PolymorphicDrawer"/>
    /// which allows the user to select its type in the Inspector.
    /// </summary>
    /// <remarks>
    /// Implement this interface in a <see cref="UnityEditor.PropertyDrawer"/> to indicate that it
    /// should entirely replace the <see cref="Editor.PolymorphicDrawer"/>.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IPolymorphic
    public interface IPolymorphic { }

    /************************************************************************************************************************/

    /// <summary>An <see cref="IPolymorphic"/> with a <see cref="Reset"/> method.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IPolymorphicReset
    public interface IPolymorphicReset : IPolymorphic
    {
        /// <summary>Called when an instance of this type is created in a [<see cref="SerializeReference"/>] field.</summary>
        void Reset(object oldValue = null);
    }

    /************************************************************************************************************************/

    /// <summary>
    /// The attributed field will be drawn by a <see cref="Editor.PolymorphicDrawer"/>
    /// which allows the user to select its type in the Inspector.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/PolymorphicAttribute
    public sealed class PolymorphicAttribute : PropertyAttribute { }

    /************************************************************************************************************************/
}

