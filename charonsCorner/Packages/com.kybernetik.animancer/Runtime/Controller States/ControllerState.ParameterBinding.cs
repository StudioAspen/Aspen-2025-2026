// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animancer
{
    /// https://kybernetik.com.au/animancer/api/Animancer/ControllerState
    partial class ControllerState
    {
        /************************************************************************************************************************/

        private SerializableParameterBindings _SerializedParameterBindings;

        /// <summary>Serialized data used to create <see cref="ParameterBinding{T}"/>s at runtime.</summary>
        public SerializableParameterBindings SerializedParameterBindings
        {
            get => _SerializedParameterBindings;
            set
            {
                _SerializedParameterBindings = value;
                DeserializeParameterBindings();
            }
        }

        /// <summary>Deserializes the <see cref="SerializedParameterBindings"/>.</summary>
        private void DeserializeParameterBindings()
        {
            if (Graph == null)
                return;

            DisposeParameterBindings();
            _SerializedParameterBindings?.Deserialize(this);
        }

        /************************************************************************************************************************/

        private List<IDisposable> _ParameterBindings;

        /// <summary>
        /// Adds an object to a list for <see cref="IDisposable.Dispose"/>
        /// to be called in <see cref="Destroy"/>.
        /// </summary>
        private void AddParameterBinding(IDisposable disposable)
        {
            _ParameterBindings ??= new();
            _ParameterBindings.Add(disposable);
        }

        /// <summary>Disposes everything added by <see cref="AddParameterBinding"/>.</summary>
        private void DisposeParameterBindings()
        {
            if (_ParameterBindings == null)
                return;

            for (int i = _ParameterBindings.Count - 1; i >= 0; i--)
                _ParameterBindings[i].Dispose();

            _ParameterBindings.Clear();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures all parameters in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public void BindAllParameters()
        {
            var count = Playable.GetParameterCount();
            for (int i = 0; i < count; i++)
            {
                var parameter = Playable.GetParameter(i);
                BindParameter(parameter.name, parameter);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public void BindParameter(StringReference name)
            => BindParameter(name, name);

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public void BindParameter(StringReference animancerParameter, string controllerParameterName)
            => BindParameter(animancerParameter, Animator.StringToHash(controllerParameterName));

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public void BindParameter(StringReference animancerParameter, int controllerParameterHash)
        {
            var count = Playable.GetParameterCount();
            for (int i = 0; i < count; i++)
            {
                var parameter = Playable.GetParameter(i);
                if (parameter.nameHash == controllerParameterHash)
                {
                    BindParameter(animancerParameter, parameter);
                    break;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures all parameters in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public void BindParameter(
            StringReference animancerParameter,
            AnimatorControllerParameter controllerParameter)
        {
            switch (controllerParameter.type)
            {
                case AnimatorControllerParameterType.Float:
                    BindFloat(animancerParameter, controllerParameter.nameHash);
                    break;

                case AnimatorControllerParameterType.Int:
                    BindInt(animancerParameter, controllerParameter.nameHash);
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    BindBool(animancerParameter, controllerParameter.nameHash);
                    break;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<bool> BindBool(StringReference name)
            => BindBool(name, name);

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<bool> BindBool(StringReference animancerParameter, string controllerParameterName)
            => BindBool(animancerParameter, Animator.StringToHash(controllerParameterName));

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<bool> BindBool(StringReference animancerParameter, int controllerParameterHash)
        {
            var parameter = Graph.Parameters.GetOrCreate<bool>(animancerParameter);
            var binding = new ParameterBinding<bool>(
                parameter,
                value => Playable.SetBool(controllerParameterHash, value));
            AddParameterBinding(binding);
            return binding;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<float> BindFloat(StringReference name)
            => BindFloat(name, name);

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<float> BindFloat(StringReference animancerParameter, string controllerParameterName)
            => BindFloat(animancerParameter, Animator.StringToHash(controllerParameterName));

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<float> BindFloat(StringReference animancerParameter, int controllerParameterHash)
        {
            var parameter = Graph.Parameters.GetOrCreate<float>(animancerParameter);
            var binding = new ParameterBinding<float>(
                parameter,
                value => Playable.SetFloat(controllerParameterHash, value));
            AddParameterBinding(binding);
            return binding;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter with the same name in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<int> BindInt(StringReference name)
            => BindInt(name, name);

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<int> BindInt(StringReference animancerParameter, string controllerParameterName)
            => BindInt(animancerParameter, Animator.StringToHash(controllerParameterName));

        /// <summary>
        /// Configures a parameter in the <see cref="Controller"/>
        /// to follow the value of a parameter in the <see cref="AnimancerGraph.Parameters"/>.
        /// </summary>
        public ParameterBinding<int> BindInt(StringReference animancerParameter, int controllerParameterHash)
        {
            var parameter = Graph.Parameters.GetOrCreate<int>(animancerParameter);
            var binding = new ParameterBinding<int>(
                parameter,
                value => Playable.SetInteger(controllerParameterHash, value));
            AddParameterBinding(binding);
            return binding;
        }

        /************************************************************************************************************************/

        /// <summary>An <see cref="IDisposable"/> binding to <see cref="Parameter{T}.OnValueChanged"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer/ParameterBinding_1
        public class ParameterBinding<T> : IDisposable
        {
            /************************************************************************************************************************/

            /// <summary>The parameter being watched.</summary>
            public readonly Parameter<T> Parameter;

            /// <summary>The callback to invoke when the parameter changes.</summary>
            public readonly Action<T> OnParameterChanged;

            /************************************************************************************************************************/

            /// <summary>
            /// Invokes `onParameterChanged` and adds it to the <see cref="Parameter{T}.OnValueChanged"/>
            /// to be removed by <see cref="Dispose"/>.
            /// </summary>
            public ParameterBinding(
                Parameter<T> parameter,
                Action<T> onParameterChanged)
            {
                Parameter = parameter;
                OnParameterChanged = onParameterChanged;

                OnParameterChanged(Parameter.Value);
                Parameter.OnValueChanged += OnParameterChanged;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Removes <see cref="OnParameterChanged"/> from the <see cref="Parameter{T}.OnValueChanged"/>.
            /// </summary>
            public void Dispose()
            {
                Parameter.OnValueChanged -= OnParameterChanged;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/

        /// <summary>
        /// A serializable array of data which can create <see cref="ParameterBinding{T}"/>s at runtime.
        /// </summary>
        /// <remarks>
        /// This data contains a <see cref="Bindings"/> array and <see cref="Mode"/> flag:
        /// <list type="bullet">
        /// 
        /// <item>
        /// If the array is empty,
        /// <c>true</c> will bind all parameters by name
        /// and <c>false</c> will bind nothing.
        /// </item>
        /// 
        /// <item>
        /// Otherwise, <c>true</c> will bind <c>[i * 2]</c> in the <see cref="RuntimeAnimatorController"/>
        /// to <c>[i * 2 + 1]</c> in the <see cref="AnimancerGraph.Parameters"/>.
        /// </item>
        /// 
        /// <item>
        /// And <c>false</c> will bind each of its parameters to the same name in both systems.
        /// </item>
        /// 
        /// </list>
        /// </remarks>
        /// https://kybernetik.com.au/animancer/api/Animancer/SerializableParameterBindings
        [Serializable]
        public class SerializableParameterBindings :
            ICloneable<SerializableParameterBindings>
        {
            /************************************************************************************************************************/

            [SerializeField]
            private bool _Mode;

            /// <summary>[<see cref="SerializeField"/>]
            /// Modifies the way the <see cref="Bindings"/> array is interpreted.
            /// </summary>
            /// <remarks>See the <see cref="SerializableParameterBindings"/> class for details.</remarks>
            public ref bool Mode
                => ref _Mode;

#if UNITY_EDITOR
            /// <summary>[Editor-Only] The name of the serialized backing field of <see cref="Mode"/>.</summary>
            public const string ModeFieldName = nameof(_Mode);
#endif

            /************************************************************************************************************************/

            /// <summary>[<see cref="SerializeField"/>]
            /// Should all parameters in the <see cref="RuntimeAnimatorController"/> be bound by name?
            /// </summary>
            /// <remarks>See the <see cref="SerializableParameterBindings"/> class for details.</remarks>
            public bool BindAllParameters
            {
                get => _Mode && _Bindings.Length == 0;
                set
                {
                    _Mode = value;

                    if (value)
                    {
                        _Bindings = Array.Empty<StringAsset>();
                    }
                    else
                    {
                        Debug.Assert(
                            _Bindings.Length == 0,
                            $"{nameof(BindAllParameters)} can't be disabled unless the {nameof(Bindings)}" +
                            $" array is empty because it changes the meaning of that array.");
                    }
                }
            }

            /************************************************************************************************************************/

            /// <summary>[<see cref="SerializeField"/>]
            /// Should the <see cref="Bindings"/> be grouped into pairs
            /// to bind each <see cref="RuntimeAnimatorController"/> parameter
            /// to the subsequent parameter in <see cref="AnimancerGraph.Parameters"/>?
            /// </summary>
            /// <remarks>See the <see cref="SerializableParameterBindings"/> class for details.</remarks>
            public bool RebindNames
            {
                get => _Mode && _Bindings.Length > 0;
                set
                {
                    _Mode = value;

                    if (value)
                    {
                        if (_Bindings.Length % 2 != 0)
                            Array.Resize(ref _Bindings, _Bindings.Length + 1);
                    }
                    else
                    {
                        Debug.Assert(
                            _Bindings.Length == 0,
                            $"{nameof(RebindNames)} can't be disabled unless the {nameof(Bindings)}" +
                            $" array is empty because it changes the meaning of that array.");
                    }
                }
            }

            /************************************************************************************************************************/

            [SerializeField]
            private StringAsset[] _Bindings = Array.Empty<StringAsset>();

            /// <summary>[<see cref="SerializeField"/>]
            /// Parameter names used to have parameters in the <see cref="RuntimeAnimatorController"/>
            /// follow the value of parameters in the <see cref="AnimancerGraph.Parameters"/>.
            /// </summary>
            /// <remarks>See the <see cref="SerializableParameterBindings"/> class for details.</remarks>
            public StringAsset[] Bindings
            {
                get => _Bindings;
                set
                {
                    Debug.Assert(
                        value != null,
                        $"{nameof(Bindings)} can't be null. Use Array.Empty<StringAsset>() instead.");

                    _Bindings = value;
                }
            }

#if UNITY_EDITOR
            /// <summary>[Editor-Only] The name of the serialized backing field of <see cref="Bindings"/>.</summary>
            public const string BindingsFieldName = nameof(_Bindings);
#endif

            /************************************************************************************************************************/

            /// <summary>Creates runtime bindings for the `state`.</summary>
            /// <remarks>See the <see cref="SerializableParameterBindings"/> class for details.</remarks>
            public void Deserialize(ControllerState state)
            {
                if (_Bindings.Length == 0)
                {
                    if (_Mode)
                        state.BindAllParameters();
                    // Else do nothing.
                }
                else
                {
                    if (_Mode)
                    {
                        for (int i = 0; i < _Bindings.Length - 1; i += 2)
                        {
                            var controller = _Bindings[i];
                            var animancer = _Bindings[i + 1];
                            if (controller == null ||
                                animancer == null)
                                continue;

                            state.BindParameter(animancer, controller);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _Bindings.Length; i++)
                        {
                            var name = _Bindings[i];
                            if (name == null)
                                continue;

                            state.BindParameter(name);
                        }
                    }
                }
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public SerializableParameterBindings Clone(CloneContext context)
            {
                var bindingCount = Bindings != null ? Bindings.Length : 0;
                var clone = new SerializableParameterBindings()
                {
                    BindAllParameters = BindAllParameters,
                    Bindings = new StringAsset[bindingCount],
                };

                for (int i = 0; i < bindingCount; i++)
                    clone.Bindings[i] = context.GetCloneOrOriginal(Bindings[i]);

                return clone;
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

