// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// <summary>An object with various parameters.</summary>
    /// <remarks>This system is inefficient and intended for Editor-Only use.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IParametizedState
    public interface IParametizedState
    {
        /************************************************************************************************************************/

        /// <summary>Gets the details of all parameters in this state.</summary>
        void GetParameters(List<StateParameterDetails> parameters);

        /// <summary>Sets the details of all parameters in this state.</summary>
        void SetParameters(List<StateParameterDetails> parameters);

        /************************************************************************************************************************/
    }

    /// <summary>Details of a parameter in an <see cref="IParametizedState"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/StateParameterDetails
    public struct StateParameterDetails
    {
        /************************************************************************************************************************/

        /// <summary>The display name.</summary>
        public string label;

        /// <summary>The <see cref="AnimancerGraph.Parameters"/> binding name.</summary>
        public string name;

        /// <summary>The type of parameter.</summary>
        public AnimatorControllerParameterType type;

        /// <summary>The current value of the parameter.</summary>
        public object value;

        /************************************************************************************************************************/

        /// <summary>A special value for <see cref="name"/> which indicates that binding is not supported.</summary>
        public const string NoBinding = nameof(NoBinding);

        /// <summary>Does the parameter support binding?</summary>
        public bool SupportsBinding
            => !ReferenceEquals(name, NoBinding);

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="StateParameterDetails"/>.</summary>
        public StateParameterDetails(
            string label,
            string name,
            AnimatorControllerParameterType type,
            object value)
        {
            this.label = label;
            this.name = name;
            this.type = type;
            this.value = value;
        }

        /// <summary>Creates a new <see cref="StateParameterDetails"/>.</summary>
        public StateParameterDetails(
            string label,
            AnimatorControllerParameterType type,
            object value)
            : this(label, NoBinding, type, value)
        { }

        /************************************************************************************************************************/
    }
}

