// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// Attribute for classes which implement <see cref="CustomGUI{T}"/> to specify the type of objects they apply to.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/CustomGUIAttribute
    /// 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class CustomGUIAttribute : Attribute
    {
        /************************************************************************************************************************/

        /// <summary>The type of object which the attributed <see cref="CustomGUI{T}"/> class applies to.</summary>
        public readonly Type TargetType;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="CustomGUIAttribute"/>.</summary>
        public CustomGUIAttribute(Type targetType)
        {
            TargetType = targetType;
        }

        /************************************************************************************************************************/
    }
}

#endif

