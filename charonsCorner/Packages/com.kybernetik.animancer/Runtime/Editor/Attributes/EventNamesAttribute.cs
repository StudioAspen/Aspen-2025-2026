// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Reflection;

#if UNITY_EDITOR
using System.Collections;
#endif

namespace Animancer
{
    /// <summary>[Editor-Conditional]
    /// Specifies a set of acceptable names for <see cref="AnimancerEvent"/>s
    /// so they can display a warning in the Inspector if an unexpected name is used.
    /// </summary>
    /// 
    /// <remarks>
    /// Placing this attribute on a type applies it to all fields in that type.
    /// <para></para>
    /// <strong>Documentation:</strong>
    /// <see href="https://kybernetik.com.au/animancer/docs/manual/events/animancer/usage#event-names">
    /// Event Names</see>
    /// <para></para>
    /// <strong>Example:</strong><code>
    /// [EventNames(...)]// Apply to all fields in this class.
    /// public class AttackState
    /// {
    ///     [SerializeField]
    ///     [EventNames(...)]// Apply to only this field.
    ///     private ClipTransition _Action;
    /// }
    /// </code>
    /// See the constructors for examples of their usage.
    /// </remarks>
    /// 
    /// https://kybernetik.com.au/animancer/api/Animancer/EventNamesAttribute
    /// 
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    [System.Diagnostics.Conditional(Strings.UnityEditor)]
    public sealed class EventNamesAttribute : Attribute
#if UNITY_EDITOR
        , IInitializable<MemberInfo>
#endif
    {
        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] The names that can be used for events in the attributed field.</summary>
        public StringReference[] Names { get; private set; }

        /// <summary>[Editor-Only] Has the <see cref="Names"/> array been initialized?</summary>
        public bool HasNames
            => !Names.IsNullOrEmpty();
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="EventNamesAttribute"/>
        /// with <see cref="Names"/> from the attributed type or declaring type of the attributed member.</summary>
        /// 
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// [EventNames]// Use all StringReference fields in this class for any transitions in this class.
        /// public class AttackState
        /// {
        ///     public static readonly StringReference HitStart = "Hit Start";
        ///     public static readonly StringReference HitEnd = "Hit End";
        /// 
        ///     [SerializeField]
        ///     [EventNames]// Use all StringReference fields in this class.
        ///     private ClipTransition _Animation;
        /// 
        ///     protected virtual void Awake()
        ///     {
        ///         _Animation.Events.SetCallback(HitStart, OnHitStart);
        ///         _Animation.Events.SetCallback(HitEnd, OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></remarks>
        public EventNamesAttribute()
        {
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="EventNamesAttribute"/> containing the specified `names`.</summary>
        /// <remarks>
        /// <strong>Example:</strong><code>
        /// public class AttackState
        /// {
        ///     [SerializeField]
        ///     [EventNames("Hit Start", "Hit End")]
        ///     private ClipTransition _Animation;
        /// 
        ///     protected virtual void Awake()
        ///     {
        ///         _Animation.Events.SetCallback("Hit Start", OnHitStart);
        ///         _Animation.Events.SetCallback("Hit End", OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></remarks>
        public EventNamesAttribute(params string[] names)
        {
#if UNITY_EDITOR
            Names = StringReference.Get(names);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="EventNamesAttribute"/> with <see cref="Names"/> from the `type`.</summary>
        /// 
        /// <remarks>
        /// If the `type` is an enum, all of its values will be used.
        /// <para></para>
        /// Otherwise the values of all static <see cref="string"/> and
        /// <see cref="StringReference"/> fields will be used.
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// public class AttackState
        /// {
        ///     public static readonly StringReference HitStart = "Hit Start";
        ///     public static readonly StringReference HitEnd = "Hit End";
        /// 
        ///     [SerializeField]
        ///     [EventNames(typeof(AttackState))]// Use all StringReference fields in this class.
        ///     private ClipTransition _Animation;
        /// 
        ///     protected virtual void Awake()
        ///     {
        ///         _Animation.Events.SetCallback(HitStart, OnHitStart);
        ///         _Animation.Events.SetCallback(HitEnd, OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></remarks>
        /// 
        /// <exception cref="ArgumentNullException"/>
        public EventNamesAttribute(Type type)
        {
#if UNITY_EDITOR
            Initialize(type);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <see cref="EventNamesAttribute"/> with <see cref="Names"/> from a member in the `type`
        /// with the specified `name`.
        /// </summary>
        /// 
        /// <remarks>
        /// The specified member must be static and can be a Field, Property, or Method.
        /// <para></para>
        /// The member type can be anything implementing <see cref="IEnumerable"/> (including arrays, lists, and
        /// coroutines).
        /// <para></para>
        /// <strong>Example:</strong><code>
        /// public class AttackState
        /// {
        ///     public static readonly StringReference[] Events = { "Hit Start", "Hit End" };
        /// 
        ///     [SerializeField]
        ///     [EventNames(typeof(AttackState), nameof(Events))]// Get the names from AttackState.Events.
        ///     private ClipTransition _Animation;
        /// 
        ///     protected virtual void Awake()
        ///     {
        ///         _Animation.Events.SetCallback(Events[0], OnHitStart);
        ///         _Animation.Events.SetCallback(Events[1], OnHitEnd);
        ///     }
        /// 
        ///     private void OnHitStart() { }
        ///     private void OnHitEnd() { }
        /// }
        /// </code></remarks>
        /// 
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">No member with the specified `name` exists in the `type`.</exception>
        /// 
        public EventNamesAttribute(Type type, string name)
        {
#if UNITY_EDITOR
            var obj = GetValue(type, name)
                ?? throw new ArgumentException(
                    $"The collection retrieved from {type.GetNameCS()}.{name} is null");

            if (obj is not IEnumerable collection)
                throw new ArgumentException(
                    $"The object retrieved from {type.GetNameCS()}.{name} is not an {nameof(IEnumerable)}");

            using (ListPool<StringReference>.Instance.Acquire(out var names))
            {
                foreach (var item in collection)
                {
                    if (item == null)
                        continue;

                    var itemName = item.ToString();
                    if (string.IsNullOrEmpty(itemName))
                        continue;

                    names.Add(itemName);
                }

                if (names.Count == 0)
                    throw new ArgumentException($"The collection retrieved from {type.GetNameCS()}.{name} is empty");

                Names = names.ToArray();
            }
#endif
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="Names"/> if they weren't already set in the constructor.</summary>
        public void Initialize(MemberInfo member)
        {
            if (HasNames)
                return;

            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (member is Type type)
            {
                Initialize(type);
            }
            else
            {
                Names = GatherNames(member.DeclaringType);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes the <see cref="Names"/> if they weren't already set in the constructor.</summary>
        public void Initialize(Type type)
        {
            if (HasNames)
                return;

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsEnum)
            {
                Names = StringReference.Get(Enum.GetNames(type));
            }
            else
            {
                Names = GatherNames(type);
            }
        }

        /************************************************************************************************************************/

        private static object GetValue(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var field = type.GetField(name, AnimancerReflection.StaticBindings);
            if (field != null)
                return field.GetValue(null);

            var property = type.GetProperty(name, AnimancerReflection.StaticBindings);
            if (property != null)
                return property.GetValue(null, null);

            var method = type.GetMethod(name, AnimancerReflection.StaticBindings, null, Type.EmptyTypes, null);
            if (method != null)
                return method.Invoke(null, null);

            throw new ArgumentException($"{type.GetNameCS()} does not contain a member named '{name}'");
        }

        /************************************************************************************************************************/

        private static StringReference[] GatherNames(Type type)
        {
            using (ListPool<StringReference>.Instance.Acquire(out var names))
            {
                while (type != null)
                {
                    var fields = type.GetFields(AnimancerReflection.StaticBindings | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];

                        if (field.DeclaringType.Assembly.FullName.StartsWith("Unity"))
                            continue;

                        StringReference name;

                        if (field.FieldType == typeof(string))
                        {
                            name = (string)field.GetValue(null);
                        }
                        else if (field.FieldType == typeof(StringReference))
                        {
                            name = (StringReference)field.GetValue(null);
                        }
                        else continue;

                        if (!name.IsNullOrEmpty() && !names.Contains(name))
                            names.Add(name);
                    }

                    type = type.BaseType;
                }

                if (names.Count == 0)
                    return null;

                names.Sort();
                return names.ToArray();
            }
        }

        /************************************************************************************************************************/

        private string _Prefix;
        private string _NamesToString;

        /// <summary>Returns a string containing all the <see cref="Names"/>.</summary>
        public string NamesToString(string prefix, string delimiter = "\n• ")
        {
            if (!HasNames)
                return prefix;

            if (_NamesToString != null && _Prefix == prefix)
                return _NamesToString;

            var text = StringBuilderPool.Instance.Acquire();

            _Prefix = prefix;
            text.Append(prefix);

            for (int i = 0; i < Names.Length; i++)
                text.Append(delimiter).Append(Names[i]);

            return _NamesToString = text.ReleaseToString();
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

