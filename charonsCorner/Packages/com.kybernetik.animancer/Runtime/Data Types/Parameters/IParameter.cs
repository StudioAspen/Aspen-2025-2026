// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>A value wrapper with utilities for being drawn in the Inspector.</summary>
    /// <remarks>This interface is used for non-generic access to <see cref="Parameter{T}"/>.</remarks>
    public interface IParameter : IComparable<IParameter>
    {
        /************************************************************************************************************************/

        /// <summary>The key this parameter is registered with in the <see cref="ParameterDictionary"/>.</summary>
        StringReference Key { get; }

        /// <summary>The current value of this parameter.</summary>
        object Value { get; set; }

        /// <summary>The type of the <see cref="Value"/>.</summary>
        Type ValueType { get; }

        /// <summary>Returns a delegate that will be invoked when the <see cref="Value"/> changes.</summary>
        /// <remarks>This is used for displaying the parameter details in the Inspector.</remarks>
        Delegate GetOnValueChanged();

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS || ANIMANCER_DEBUG_PARAMETERS
        /************************************************************************************************************************/

        /// <summary>[Assert-Only] If set, all interactions with this parameter will be logged with this prefix.</summary>
        /// <remarks>
        /// If a <see cref="UnityEngine.Object"/> is assigned, it will also be used as the context for the logs,
        /// meaning you can click the messages in the Console window to highlight that object in the Hierarchy.
        /// <para></para>
        /// Logs will also include the parameter's key so you can assign <c>""</c> if you don't need to differentiate
        /// between different objects which use the same name.
        /// </remarks>
        object LogContext { get; set; }

        /// <summary>[Assert-Only]
        /// If true, attempts to set the <see cref="Value"/> in code will be ignored so this parameter can only be
        /// controlled manually in the Inspector.
        /// </summary>
        bool InspectorControlOnly { get; set; }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

