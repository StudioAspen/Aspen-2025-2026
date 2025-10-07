// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerEvent
    partial struct AnimancerEvent
    {
        /************************************************************************************************************************/
        // Reference Types.
        /************************************************************************************************************************/

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="UnityEngine.Object"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterObject
        [Serializable]
        public class ParameterObject : Parameter<UnityEngine.Object> { }

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="string"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterString
        [Serializable]
        public class ParameterString : Parameter<string> { }

        /************************************************************************************************************************/
        // Value Types.
        /************************************************************************************************************************/

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="bool"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterBool
        [Serializable]
        public class ParameterBool : ParameterBoxed<bool> { }

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="double"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterDouble
        [Serializable]
        public class ParameterDouble : ParameterBoxed<double> { }

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="float"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterFloat
        [Serializable]
        public class ParameterFloat : ParameterBoxed<float> { }

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="int"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterInt
        [Serializable]
        public class ParameterInt : ParameterBoxed<int> { }

        /// <summary>An <see cref="Parameter{T}"/> for <see cref="long"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterLong
        [Serializable]
        public class ParameterLong : ParameterBoxed<long> { }

        /************************************************************************************************************************/
    }
}

