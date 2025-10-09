// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;

namespace Animancer
{
    /// <summary>A dictionary which maps event names to callbacks.</summary>
    /// <remarks>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer">
    /// Animancer Events</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/NamedEventDictionary
    public class NamedEventDictionary : IDictionary<StringReference, Action>
    {
        /************************************************************************************************************************/

        private readonly Dictionary<StringReference, Action>
            Dictionary = new();

        /************************************************************************************************************************/

        /// <summary>The number of items in this dictionary.</summary>
        public int Count
            => Dictionary.Count;

        /************************************************************************************************************************/
        #region Access
        /************************************************************************************************************************/

        /// <summary>Accesses a callback in this dictionary.</summary>
        public Action this[StringReference name]
        {
            get => Dictionary[name];
            set
            {
                AssertNotEndEvent(name);
                Dictionary[name] = value;
            }
        }

        /************************************************************************************************************************/

        /// <summary>Returns the callback registered using the `name`.</summary>
        /// <remarks>Returns <c>null</c> if nothing was registered.</remarks>
        public Action Get(StringReference name)
            => Dictionary.Get(name);

        /// <summary>Registers the callback using the `name`, replacing anything previously registered.</summary>
        public void Set(StringReference name, Action callback)
        {
            AssertNotEndEvent(name);
            Dictionary[name] = callback;
        }

        /************************************************************************************************************************/

        /// <summary>Are any callbacks registered for the `name`?</summary>
        /// <remarks>To get the registered callbacks at the same time, use <see cref="TryGetValue"/> instead.</remarks>
        public bool ContainsKey(StringReference name)
            => Dictionary.ContainsKey(name);

        /************************************************************************************************************************/

        /// <summary>Tries to get the `callback` registered with the `name` and returns <c>true</c> if successful.</summary>
        public bool TryGetValue(StringReference name, out Action callback)
            => Dictionary.TryGetValue(name, out callback);

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Add
        /************************************************************************************************************************/

        /// <summary>Adds the `callback` to any existing ones registered with the `name`.</summary>
        /// <remarks>
        /// If you want an exception to be thrown if something is already registered with the `name`,
        /// use <see cref="AddNew(StringReference, Action)"/> instead.
        /// </remarks>
        public void AddTo(StringReference name, Action callback)
        {
            AssertNotEndEvent(name);

            if (Dictionary.TryGetValue(name, out var existing))
                callback = existing + callback;

            Dictionary[name] = callback;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Registers the `callback` with the `name` but throws an <see cref="ArgumentException"/>
        /// if something was already registered with the same `name`.
        /// </summary>
        /// <remarks>
        /// This matches the standard <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/> behaviour,
        /// unlike <see cref="AddTo(StringReference, Action)"/>.
        /// </remarks>
        public void AddNew(StringReference name, Action callback)
        {
            AssertNotEndEvent(name);
            Dictionary.Add(name, callback);
        }

        void IDictionary<StringReference, Action>.Add(StringReference name, Action callback)
            => AddNew(name, callback);

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the `callback` to any existing ones registered with the `name`.
        /// <para></para>
        /// It will be invoked using <see cref="AnimancerEvent.GetCurrentParameter{T}"/> to get its parameter.
        /// </summary>
        /// <remarks>
        /// If you want an exception to be thrown if something is already registered with the `name`,
        /// use <see cref="AddNew{T}(StringReference, Action{T})"/> instead.
        /// <para></para>
        /// If <typeparamref name="T"/> is <see cref="string"/>,
        /// consider using <see cref="AddTo(StringReference, Action{string})"/> instead of this overload.
        /// <para></para>
        /// If you want to later remove the `callback`,
        /// you need to store and remove the returned <see cref="Action"/>.
        /// </remarks>
        public Action AddTo<T>(StringReference name, Action<T> callback)
        {
            AssertNotEndEvent(name);
            var parametized = AnimancerEvent.Parametize(callback);

            if (Dictionary.TryGetValue(name, out var existing))
                parametized = existing + parametized;

            Dictionary[name] = parametized;

            return parametized;
        }

        /// <summary>
        /// Registers the `callback` with the `name` but throws an <see cref="ArgumentException"/>
        /// if something was already registered with the same `name`.
        /// <para></para>
        /// It will be invoked using <see cref="AnimancerEvent.GetCurrentParameter{T}"/> to get its parameter.
        /// </summary>
        /// <remarks>
        /// This matches the standard <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/> behaviour,
        /// unlike <see cref="AddTo{T}(StringReference, Action{T})"/>.
        /// If <typeparamref name="T"/> is <see cref="string"/>,
        /// consider using <see cref="AddTo(StringReference, Action{string})"/> instead of this overload.
        /// <para></para>
        /// If you want to later remove the `callback`,
        /// you need to store and remove the returned <see cref="Action"/>.
        /// </remarks>
        public Action AddNew<T>(StringReference name, Action<T> callback)
        {
            AssertNotEndEvent(name);
            var parametized = AnimancerEvent.Parametize(callback);
            Dictionary.Add(name, parametized);
            return parametized;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Adds the `callback` to any existing ones registered with the `name`.
        /// <para></para>
        /// It will be invoked using <see cref="object.ToString"/> on the
        /// <see cref="AnimancerEvent.CurrentParameter"/>.
        /// </summary>
        /// <remarks>
        /// If you want an exception to be thrown if something is already registered with the `name`,
        /// use <see cref="AddNew{T}(StringReference, Action{T})"/> instead.
        /// <para></para>
        /// If you want to later remove the `callback`,
        /// you need to store and remove the returned <see cref="Action"/>.
        /// </remarks>
        public Action AddTo(StringReference name, Action<string> callback)
        {
            AssertNotEndEvent(name);
            var parametized = AnimancerEvent.Parametize(callback);

            if (Dictionary.TryGetValue(name, out var existing))
                parametized = existing + parametized;

            Dictionary[name] = parametized;
            return parametized;
        }

        /// <summary>
        /// Registers the `callback` with the `name` but throws an <see cref="ArgumentException"/>
        /// if something was already registered with the same `name`.
        /// <para></para>
        /// It will be invoked using <see cref="object.ToString"/> on the
        /// <see cref="AnimancerEvent.CurrentParameter"/>.
        /// </summary>
        /// <remarks>
        /// This matches the standard <see cref="Dictionary{TKey, TValue}.Add(TKey, TValue)"/>
        /// behaviour, unlike <see cref="AddTo(StringReference, Action{string})"/>.
        /// <para></para>
        /// If you want to later remove the `callback`,
        /// you need to store and remove the returned <see cref="Action"/>.
        /// </remarks>
        public Action AddNew(StringReference name, Action<string> callback)
        {
            AssertNotEndEvent(name);
            var parametized = AnimancerEvent.Parametize(callback);
            Dictionary.Add(name, parametized);
            return parametized;
        }

        /************************************************************************************************************************/

        /// <summary>[Assert-Conditional]
        /// Throws an <see cref="ArgumentException"/> if the `name` is the <see cref="AnimancerEvent.EndEventName"/>.
        /// </summary>
        /// <remarks>
        /// In order to minimise the performance cost of End Events when there isn't one,
        /// the <see cref="AnimancerEvent.Dispatcher"/> won't even check the end time
        /// when there is no <see cref="AnimancerEvent.Sequence.OnEnd"/> callback.
        /// <para></para>
        /// That means if a callback was bound to the <see cref="AnimancerEvent.EndEventName"/>
        /// it would be triggered by any state with an <see cref="AnimancerEvent.Sequence.OnEnd"/>
        /// callback, but not by states without one. That would be very counterintuitive so it isn't allowed.
        /// </remarks>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assertions)]
        public static void AssertNotEndEvent(StringReference name)
        {
            if (name == AnimancerEvent.EndEventName)
                throw new ArgumentException(
                    $"Binding event callbacks to the " +
                    $"{nameof(AnimancerEvent)}.{nameof(AnimancerEvent.EndEventName)}" +
                    $" is not supported for performance optimization reasons. See the documentation of" +
                    $" {nameof(NamedEventDictionary)}.{nameof(AssertNotEndEvent)} for more details.",
                    nameof(name));
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Remove
        /************************************************************************************************************************/

        /// <summary>Removes all callbacks registered with the `name`.</summary>
        public bool Remove(StringReference name)
            => Dictionary.Remove(name);

        /// <summary>Removes a specific `callback` registered with the `name`.</summary>
        public bool Remove(StringReference name, Action callback)
        {
            if (!Dictionary.TryGetValue(name, out var callbacks))
                return false;

            if (callbacks == callback)
                Dictionary.Remove(name);
            else
                Dictionary[name] = callbacks - callback;

            return true;
        }

        /************************************************************************************************************************/

        /// <summary>Removes everything from this dictionary.</summary>
        public void Clear()
            => Dictionary.Clear();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Enumeration
        /************************************************************************************************************************/

        /// <summary>Returns an enumerator to go through every item in this dictionary.</summary>
        public Dictionary<StringReference, Action>.Enumerator GetEnumerator()
            => Dictionary.GetEnumerator();

        IEnumerator<KeyValuePair<StringReference, Action>>
            IEnumerable<KeyValuePair<StringReference, Action>>.GetEnumerator()
            => Dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Dictionary.GetEnumerator();

        /************************************************************************************************************************/

        /// <summary>The names in this dictionary.</summary>
        public Dictionary<StringReference, Action>.KeyCollection Keys
            => Dictionary.Keys;

        /// <summary>The values in this dictionary.</summary>
        public Dictionary<StringReference, Action>.ValueCollection Values
            => Dictionary.Values;

        ICollection<StringReference> IDictionary<StringReference, Action>.Keys
            => Dictionary.Keys;

        ICollection<Action> IDictionary<StringReference, Action>.Values
            => Dictionary.Values;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Explicit Dictionary Wrappers
        /************************************************************************************************************************/

        void ICollection<KeyValuePair<StringReference, Action>>
            .Add(KeyValuePair<StringReference, Action> item)
            => AddTo(item.Key, item.Value);

        bool ICollection<KeyValuePair<StringReference, Action>>
            .Contains(KeyValuePair<StringReference, Action> item)
            => ((ICollection<KeyValuePair<StringReference, Action>>)Dictionary).Contains(item);

        void ICollection<KeyValuePair<StringReference, Action>>
            .CopyTo(KeyValuePair<StringReference, Action>[] array,
            int arrayIndex)
            => ((ICollection<KeyValuePair<StringReference, Action>>)Dictionary).CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<StringReference, Action>>
            .Remove(KeyValuePair<StringReference, Action> item)
            => Dictionary.Remove(item.Key);

        bool ICollection<KeyValuePair<StringReference, Action>>.IsReadOnly
            => false;

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

