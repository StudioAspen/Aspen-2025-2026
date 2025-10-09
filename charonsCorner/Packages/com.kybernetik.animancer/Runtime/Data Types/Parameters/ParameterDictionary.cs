// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

//#define ANIMANCER_DEBUG_PARAMETERS

using System;
using System.Collections;
using System.Collections.Generic;

namespace Animancer
{
    /// <summary>A dictionary of <see cref="IParameter"/>s registered using <see cref="StringReference"/>s.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ParameterDictionary
    public class ParameterDictionary : IEnumerable<IParameter>
    {
        /************************************************************************************************************************/

#if UNITY_ASSERTIONS || ANIMANCER_DEBUG_PARAMETERS
        /// <summary>[Assert-Only] Is the Inspector currently being drawn?</summary>
        internal static bool IsDrawingInspector { get; set; }
#endif

        /************************************************************************************************************************/

        // Any object type could be allowed as a key,
        // but that would require more overloads of all keyed methods
        // to ensure StringReferences are used instead of raw strings.

        private readonly Dictionary<object, IParameter>
            KeyToParameter = new();

        /************************************************************************************************************************/

        /// <summary>The number of parameters that have been registered.</summary>
        public int Count
            => KeyToParameter.Count;

        /************************************************************************************************************************/

        /// <summary>Tries to get a `parameter` registered with the `key`.</summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public bool TryGet(StringReference key, out IParameter parameter)
            => key.String.Length == 0// Let null throw an exception.
            ? throw new ArgumentException("Must not be null or empty", nameof(key))
            : KeyToParameter.TryGetValue(key, out parameter);

        /// <summary>Tries to get a `parameter` registered with the `key` and verifies its type.</summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public bool TryGet<T>(StringReference key, out Parameter<T> parameter)
        {
            if (!TryGet(key, out var iParameter))
            {
                parameter = null;
                return false;
            }

            parameter = iParameter as Parameter<T>;
            if (parameter != null)
                return true;

            throw new InvalidCastException(
                $"The key '{key}' was already used to register a " +
                $"{iParameter.ValueType.FullName} parameter so it can't also be used for a " +
                $"{typeof(T).FullName} parameter.");
        }

        /************************************************************************************************************************/

        /// <summary>Gets an existing parameter registered with the `key` or creates one if necessary.</summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public Parameter<T> GetOrCreate<T>(StringReference key)
        {
            if (TryGet<T>(key, out var parameter))
                return parameter;

            parameter = new(key);
            KeyToParameter.Add(key, parameter);
            return parameter;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Gets the value of the parameter registered with the `key`.
        /// Returns the default value if no such parameter exists.
        /// </summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public T GetValue<T>(StringReference key)
            => TryGet<T>(key, out var parameter)
            ? parameter.Value
            : default;

        /// <summary>
        /// Gets the value of a <see cref="float"/> parameter registered with the `key`.
        /// Returns 0 if no such parameter exists.
        /// </summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public float GetFloat(StringReference key)
            => GetValue<float>(key);

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the `value` of the parameter registered with the `key`.
        /// Creates the parameter if it didn't exist yet.
        /// </summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public void SetValue<T>(StringReference key, T value)
            => GetOrCreate<T>(key).Value = value;

        /************************************************************************************************************************/

        /// <summary>
        /// Adds an <see cref="Parameter{T}.OnValueChanged"/> callback to the parameter registered with the `key`.
        /// Creates the parameter if it didn't exist yet.
        /// </summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public void AddOnValueChanged<T>(
            StringReference key,
            Action<T> onValueChanged,
            bool invokeImmediately = false)
        {
            var parameter = GetOrCreate<T>(key);
            parameter.OnValueChanged += onValueChanged;

            if (invokeImmediately)
                onValueChanged(parameter.Value);
        }

        /// <summary>
        /// Removes an <see cref="Parameter{T}.OnValueChanged"/> callback to the parameter registered with the `key`.
        /// </summary>
        /// <remarks>The `key` must not be null or empty.</remarks>
        public void RemoveOnValueChanged<T>(
            StringReference key,
            Action<T> onValueChanged)
        {
            if (TryGet<T>(key, out var parameter))
                parameter.OnValueChanged -= onValueChanged;
        }

        /************************************************************************************************************************/

        /// <summary>Returns an enumerator that iterates through all registered parameters.</summary>
        public Dictionary<object, IParameter>.ValueCollection.Enumerator GetEnumerator()
            => KeyToParameter.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator()
            => KeyToParameter.Values.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
            => KeyToParameter.Values.GetEnumerator();

        /// <summary>All registered keys.</summary>
        public Dictionary<object, IParameter>.KeyCollection Keys
            => KeyToParameter.Keys;

        /// <summary>All registered parameters.</summary>
        public Dictionary<object, IParameter>.ValueCollection Parameters
            => KeyToParameter.Values;

        /************************************************************************************************************************/
    }
}

