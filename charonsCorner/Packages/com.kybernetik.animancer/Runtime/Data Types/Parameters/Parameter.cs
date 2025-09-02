// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_ASSERTIONS
#define ANIMANCER_DEBUG_PARAMETERS
#endif

using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>A generic value with an event for when it gets changed.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/Parameter_1
    public class Parameter<T> : IParameter
    {
        /************************************************************************************************************************/

        private Action<T> _OnValueChanged;

        /// <summary>Called whenever the <see cref="Value"/> is changed.</summary>
        public event Action<T> OnValueChanged
        {
            add
            {
                _OnValueChanged += value;

#if ANIMANCER_DEBUG_PARAMETERS
                LogOnValueChangedRegistration('+', value);
#endif
            }
            remove
            {
                _OnValueChanged -= value;

#if ANIMANCER_DEBUG_PARAMETERS
                LogOnValueChangedRegistration('-', value);
#endif
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        Delegate IParameter.GetOnValueChanged()
            => _OnValueChanged;

        /************************************************************************************************************************/

        private T _Value;

        /// <summary>The current value of this parameter.</summary>
        /// <remarks>Setting this value invokes <see cref="OnValueChanged"/>.</remarks>
        public T Value
        {
            get
            {
#if ANIMANCER_DEBUG_PARAMETERS
                if (!ParameterDictionary.IsDrawingInspector)
                    LogValueGet();
#endif

                return _Value;
            }
            set
            {
                if (EqualityComparer<T>.Default.Equals(_Value, value))
                    return;

#if ANIMANCER_DEBUG_PARAMETERS
                if (InspectorControlOnly && !ParameterDictionary.IsDrawingInspector)
                    return;

                LogValueSet(value);
#endif

                _Value = value;

                _OnValueChanged?.Invoke(value);
            }
        }

        /// <inheritdoc/>
        object IParameter.Value
        {
            get => Value;
            set => Value = (T)value;
        }

        /// <summary>Returns the <see cref="Value"/>.</summary>
        public static implicit operator T(Parameter<T> parameter)
            => parameter != null
            ? parameter.Value
            : default;

        /************************************************************************************************************************/

        /// <summary>Gets the <see cref="Value"/>.</summary>
        /// <remarks>This is exactly the same as the property, but being a method allows it to be used as a delegate.</remarks>
        public T GetValue(T value)
            => Value;

        /// <summary>Sets the <see cref="Value"/>.</summary>
        /// <remarks>This is exactly the same as the property, but being a method allows it to be used as a delegate.</remarks>
        public void SetValue(T value)
            => Value = value;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public Type ValueType
            => typeof(T);

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public StringReference Key { get; }

        /// <summary>Compares the <see cref="Key"/>s.</summary>
        int IComparable<IParameter>.CompareTo(IParameter other)
            => StringComparer.CurrentCulture.Compare(Key, other.Key);

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="Parameter{T}"/>.</summary>
        public Parameter(StringReference key)
        {
            Key = key;
        }

        /// <summary>Creates a new <see cref="Parameter{T}"/> with the specified starting `value`.</summary>
        public Parameter(StringReference key, T value)
        {
            Key = key;
            _Value = value;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a string describing this parameter.</summary>
        public override string ToString()
            => $"{nameof(Parameter<T>)}<{typeof(T).GetNameCS()}>({Key} : {Value})";

        /************************************************************************************************************************/
        #region Debug
#if ANIMANCER_DEBUG_PARAMETERS
        /************************************************************************************************************************/

        /// <inheritdoc/>
        public object LogContext { get; set; }

        /// <inheritdoc/>
        public bool InspectorControlOnly { get; set; }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Logs a message indicating that a new listener has been added or removed from <see cref="OnValueChanged"/>.
        /// </summary>
        private void LogOnValueChangedRegistration(char operation, Action<T> listener)
        {
            if (LogContext is null)
                return;

            var text = AcquireStringBuilderWithPrefix();

            text.Append($".{nameof(OnValueChanged)} ")
                .Append(operation)
                .Append("= ")
                .Append(listener != null
                    ? AnimancerReflection.GetFullName(listener.Method)
                    : "null");

            UnityEngine.Debug.Log(
                text.ReleaseToString(),
                LogContext as Object);
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Logs a message indicating that the <see cref="Value"/> has been accessed.
        /// </summary>
        private void LogValueGet()
        {
            if (LogContext is null)
                return;

            var text = AcquireStringBuilderWithPrefix();

            text.Append(" get Value ")
                .Append(_Value);

            UnityEngine.Debug.Log(
                text.ReleaseToString(),
                LogContext as Object);
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Logs a message indicating that the <see cref="Value"/> has been changed.
        /// </summary>
        private void LogValueSet(T value)
        {
            if (LogContext is null)
                return;

            var listeners = AnimancerReflection.GetInvocationList(_OnValueChanged);

            var text = AcquireStringBuilderWithPrefix();

            text.Append(" changed from '")
                .Append(_Value)
                .Append("' to '")
                .Append(value)
                .Append("' with ")
                .Append(listeners.Length)
                .Append(" event listeners:");

            for (int i = 0; i < listeners.Length; i++)
            {
                var listener = listeners[i];
                text.AppendLine()
                    .Append(" - Target: '")
                    .Append(listener.Target)
                    .Append("', Method: ")
                    .Append(listener.Method.DeclaringType.FullName)
                    .Append('.')
                    .Append(listener.Method.Name);
            }

            UnityEngine.Debug.Log(
                text.ReleaseToString(),
                LogContext as Object);
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Only]
        /// Acquires a pooled <see cref="System.Text.StringBuilder"/> and appends the standard prefix to describe
        /// this parameter.
        /// </summary>
        private System.Text.StringBuilder AcquireStringBuilderWithPrefix()
        {
            var text = StringBuilderPool.Instance.Acquire();
            text.Append(LogContext);

            if (text.Length > 0)
                text.Append(": ");

            text.Append("Parameter<")
                .Append(typeof(T).GetNameCS())
                .Append(">(")
                .Append(Key)
                .Append(')');

            return text;
        }

        /************************************************************************************************************************/
#endif
        #endregion
        /************************************************************************************************************************/
    }
}

