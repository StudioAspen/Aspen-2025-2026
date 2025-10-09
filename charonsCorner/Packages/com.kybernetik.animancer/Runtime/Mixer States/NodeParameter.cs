// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>A wrapper for managing a <see cref="Parameter{T}"/> in an <see cref="AnimancerNode"/>.</summary>
    /// <remarks>This type is mostly intended for internal use within Mixers.</remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/NodeParameter_1
    public struct NodeParameter<T>
    {
        /************************************************************************************************************************/

        /// <summary>The node that owns this parameter.</summary>
        public AnimancerNode Node { get; private set; }

        /// <summary>The callback to invoke when the parameter changes.</summary>
        public event Action<T> OnParameterChanged;

        /************************************************************************************************************************/

        /// <summary>Has this <see cref="NodeParameter{T}"/> been constructed properly?</summary>
        public readonly bool IsInitialized
            => Node != null;

        /************************************************************************************************************************/

        private StringReference _Key;

        /// <summary>
        /// This will be used as a key in the <see cref="ParameterDictionary"/>
        /// so any changes to that parameter will invoke <see cref="OnParameterChanged"/>.
        /// </summary>
        public StringReference Key
        {
            readonly get => _Key;
            set
            {
                if (_Key.EqualsWhereEmptyIsNull(value))
                    return;

                UnBind();

                _Key = value;

                Bind();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Sets the <see cref="Key"/> and returns <c>true</c> if <see cref="Initialize"/> needs to be called.</summary>
        public bool SetKeyCheckNeedsInitialize(StringReference key)
        {
            if (_Key.EqualsWhereEmptyIsNull(key))
                return false;

            if (IsInitialized)
            {
                UnBind();

                _Key = key;

                Bind();

                return false;
            }
            else
            {
                _Key = key;
                return true;
            }
        }

        /// <summary>Initializes and binds the parameter.</summary>
        public void Initialize(AnimancerNode node, Action<T> onParameterChanged)
        {
            Node = node;
            OnParameterChanged = onParameterChanged;
            Bind();
        }

        /************************************************************************************************************************/

        /// <summary>Registers to the <see cref="AnimancerGraph.Parameters"/>.</summary>
        public readonly void Bind()
        {
            if (Node.Graph != null && !_Key.IsNullOrEmpty())
                Node.Graph.Parameters.AddOnValueChanged(_Key, OnParameterChanged, true);
        }

        /// <summary>Registers to the <see cref="AnimancerGraph.Parameters"/> if <see cref="IsInitialized"/>.</summary>
        public readonly void BindIfInitialized()
        {
            if (IsInitialized)
                Bind();
        }

        /************************************************************************************************************************/

        /// <summary>Unregisters from the <see cref="AnimancerGraph.Parameters"/>.</summary>
        public readonly void UnBind()
        {
            if (Node.Graph != null && !_Key.IsNullOrEmpty())
                Node.Graph.Parameters.RemoveOnValueChanged(_Key, OnParameterChanged);
        }

        /// <summary>Unregisters from the <see cref="AnimancerGraph.Parameters"/> if <see cref="IsInitialized"/>.</summary>
        public readonly void UnBindIfInitialized()
        {
            if (IsInitialized)
                UnBind();
        }

        /************************************************************************************************************************/
    }
}

