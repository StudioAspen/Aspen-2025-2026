// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;

namespace Animancer
{
    /// <summary>Reflection utilities used throughout Animancer.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerReflection
    public static class AnimancerReflection
    {
        /************************************************************************************************************************/

        /// <summary>Commonly used <see cref="BindingFlags"/> combinations.</summary>
        public const BindingFlags
            AnyAccessBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
            InstanceBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            StaticBindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new instance of the `type` using its parameterless constructor if it has one or a fully
        /// uninitialized object if it doesn't. Or returns <c>null</c> if the <see cref="Type.IsAbstract"/>.
        /// </summary>
        public static object CreateDefaultInstance(Type type)
        {
            if (type == null ||
                type.IsAbstract)
                return default;

            var constructor = type.GetConstructor(InstanceBindings, null, Type.EmptyTypes, null);
            if (constructor != null)
                return constructor.Invoke(null);

            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Creates a <typeparamref name="T"/> using its parameterless constructor if it has one or a fully
        /// uninitialized object if it doesn't. Or returns <c>null</c> if the <see cref="Type.IsAbstract"/>.
        /// </summary>
        public static T CreateDefaultInstance<T>()
            => (T)CreateDefaultInstance(typeof(T));

        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Returns the first <typeparamref name="TAttribute"/> attribute on the `member`
        /// or <c>null</c> if there is none.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(
            this ICustomAttributeProvider member,
            bool inherit = false)
            where TAttribute : class
        {
            var type = typeof(TAttribute);
            return member.IsDefined(type, inherit)
                ? (TAttribute)member.GetCustomAttributes(type, inherit)[0]
                : null;
        }

        /************************************************************************************************************************/

        /// <summary>Invokes a method with the specified `methodName` if it exists on the `obj`.</summary>
        [Obfuscation(Exclude = true)]// Obfuscation seems to break IL2CPP Android builds here.
        public static object TryInvoke(
            object obj,
            string methodName,
            BindingFlags bindings = InstanceBindings | BindingFlags.FlattenHierarchy,
            Type[] parameterTypes = null,
            object[] parameters = null)
        {
            if (obj == null)
                return null;

            parameterTypes ??= Type.EmptyTypes;

            var method = obj.GetType().GetMethod(methodName, bindings, null, parameterTypes, null);
            return method?.Invoke(obj, parameters);
        }

        /************************************************************************************************************************/
        #region Delegates
        /************************************************************************************************************************/

        /// <summary>Returns a string describing the details of the `method`.</summary>
        public static string ToStringDetailed<T>(
            this T method,
            bool includeType = false)
            where T : Delegate
        {
            var text = StringBuilderPool.Instance.Acquire();
            text.AppendDelegate(method, includeType);
            return text.ReleaseToString();
        }

        /// <summary>Appends the details of the `method` to the `text`.</summary>
        public static StringBuilder AppendDelegate<T>(
            this StringBuilder text,
            T method,
            bool includeType = false)
            where T : Delegate
        {
            var type = method != null
                ? method.GetType()
                : typeof(T);

            if (method == null)
            {
                return includeType
                    ? text.Append("Null(")
                        .Append(type.GetNameCS())
                        .Append(')')
                    : text.Append("Null");
            }

            if (includeType)
                text.Append(type.GetNameCS())
                    .Append('(');

            if (method.Target != null)
                text.Append("Method: ");

            text.Append(method.Method.DeclaringType.GetNameCS())
                .Append('.')
                .Append(method.Method.Name);

            if (method.Target != null)
                text.Append(", Target: '")
                    .Append(method.Target)
                    .Append("'");

            if (includeType)
                text.Append(')');

            return text;
        }

        /************************************************************************************************************************/

        /// <summary>Returns the `method`'s <c>DeclaringType.Name</c>.</summary>
        public static string GetFullName(MethodInfo method)
            => $"{method.DeclaringType.Name}.{method.Name}";

        /************************************************************************************************************************/

        private static FieldInfo _DelegatesField;
        private static bool _GotDelegatesField;

        /// <summary>
        /// Uses reflection to achieve the same as <see cref="Delegate.GetInvocationList"/> without allocating
        /// garbage every time.
        /// <list type="bullet">
        /// <item>If the delegate is <c>null</c> or , this method returns <c>false</c> and outputs <c>null</c>.</item>
        /// <item>If the underlying <c>delegate</c> field was not found, this method returns <c>false</c> and outputs <c>null</c>.</item>
        /// <item>If the delegate is not multicast, this method this method returns <c>true</c> and outputs <c>null</c>.</item>
        /// <item>If the delegate is multicast, this method this method returns <c>true</c> and outputs its invocation list.</item>
        /// </list>
        /// </summary>
        public static bool TryGetInvocationListNonAlloc(MulticastDelegate multicast, out Delegate[] delegates)
        {
            if (multicast == null)
            {
                delegates = null;
                return false;
            }

            if (!_GotDelegatesField)
            {
                const string FieldName = "delegates";

                _GotDelegatesField = true;
                _DelegatesField = typeof(MulticastDelegate).GetField("delegates",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

                if (_DelegatesField != null && _DelegatesField.FieldType != typeof(Delegate[]))
                    _DelegatesField = null;

                if (_DelegatesField == null)
                    Debug.LogError($"Unable to find {nameof(MulticastDelegate)}.{FieldName} field.");
            }

            if (_DelegatesField == null)
            {
                delegates = null;
                return false;
            }
            else
            {
                delegates = (Delegate[])_DelegatesField.GetValue(multicast);
                return true;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Tries to use <see cref="TryGetInvocationListNonAlloc"/>.
        /// Otherwise uses the regular <see cref="MulticastDelegate.GetInvocationList"/>.
        /// </summary>
        public static Delegate[] GetInvocationList(MulticastDelegate multicast)
            => TryGetInvocationListNonAlloc(multicast, out var delegates) && delegates != null
            ? delegates
            : multicast?.GetInvocationList();

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Type Names
        /************************************************************************************************************************/

        private static readonly Dictionary<Type, string>
            TypeNames = new()
            {
                { typeof(object), "object" },
                { typeof(void), "void" },
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(sbyte), "sbyte" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(short), "short" },
                { typeof(int), "int" },
                { typeof(long), "long" },
                { typeof(ushort), "ushort" },
                { typeof(uint), "uint" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(decimal), "decimal" },
            };

        private static readonly Dictionary<Type, string>
            FullTypeNames = new(TypeNames);

        /************************************************************************************************************************/

        /// <summary>Returns the name of the `type` as it would appear in C# code.</summary>
        /// <remarks>
        /// Returned values are stored in a dictionary to speed up repeated use.
        /// <para></para>
        /// <strong>Example:</strong>
        /// <para></para>
        /// <c>typeof(List&lt;float&gt;).FullName</c> would give you:
        /// <para></para>
        /// <c>System.Collections.Generic.List`1[[System.Single, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]</c>
        /// <para></para>
        /// This method would instead return <c>System.Collections.Generic.List&lt;float&gt;</c> if `fullName` is <c>true</c>, or
        /// just <c>List&lt;float&gt;</c> if it is <c>false</c>.
        /// </remarks>
        public static string GetNameCS(this Type type, bool fullName = true)
        {
            if (type == null)
                return "null";

            // Check if we have already got the name for that type.
            var names = fullName
                ? FullTypeNames
                : TypeNames;

            if (names.TryGetValue(type, out var name))
                return name;

            var text = StringBuilderPool.Instance.Acquire();

            if (type.IsArray)// Array = TypeName[].
            {
                text.Append(type.GetElementType().GetNameCS(fullName));

                text.Append('[');
                var dimensions = type.GetArrayRank();
                while (dimensions-- > 1)
                    text.Append(',');
                text.Append(']');

                goto Return;
            }

            if (type.IsPointer)// Pointer = TypeName*.
            {
                text.Append(type.GetElementType().GetNameCS(fullName));
                text.Append('*');

                goto Return;
            }

            if (type.IsGenericParameter)// Generic Parameter = TypeName (for unspecified generic parameters).
            {
                text.Append(type.Name);
                goto Return;
            }

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)// Nullable = TypeName != null ?
            {
                text.Append(underlyingType.GetNameCS(fullName));
                text.Append('?');

                goto Return;
            }

            // Other Type = Namespace.NestedTypes.TypeName<GenericArguments>.

            if (fullName && type.Namespace != null)// Namespace.
            {
                text.Append(type.Namespace);
                text.Append('.');
            }

            var genericArguments = 0;

            if (type.DeclaringType != null)// Account for Nested Types.
            {
                // Count the nesting level.
                var nesting = 1;
                var declaringType = type.DeclaringType;
                while (declaringType.DeclaringType != null)
                {
                    declaringType = declaringType.DeclaringType;
                    nesting++;
                }

                // Append the name of each outer type, starting from the outside.
                while (nesting-- > 0)
                {
                    // Walk out to the current nesting level.
                    // This avoids the need to make a list of types in the nest or to insert type names instead of appending them.
                    declaringType = type;
                    for (int i = nesting; i >= 0; i--)
                        declaringType = declaringType.DeclaringType;

                    // Nested Type Name.
                    genericArguments = AppendNameAndGenericArguments(text, declaringType, fullName, genericArguments);
                    text.Append('.');
                }
            }

            // Type Name.
            AppendNameAndGenericArguments(text, type, fullName, genericArguments);

            Return:// Remember and return the name.
            name = text.ReleaseToString();
            names.Add(type, name);
            return name;
        }

        /************************************************************************************************************************/

        /// <summary>Appends the generic arguments of `type` (after skipping the specified number).</summary>
        public static int AppendNameAndGenericArguments(StringBuilder text, Type type, bool fullName = true, int skipGenericArguments = 0)
        {
            var name = type.Name;
            text.Append(name);

            if (type.IsGenericType)
            {
                var backQuote = name.IndexOf('`');
                if (backQuote >= 0)
                {
                    text.Length -= name.Length - backQuote;

                    var genericArguments = type.GetGenericArguments();
                    if (skipGenericArguments < genericArguments.Length)
                    {
                        text.Append('<');

                        var firstArgument = genericArguments[skipGenericArguments];
                        skipGenericArguments++;

                        if (firstArgument.IsGenericParameter)
                        {
                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(',');
                                skipGenericArguments++;
                            }
                        }
                        else
                        {
                            text.Append(firstArgument.GetNameCS(fullName));

                            while (skipGenericArguments < genericArguments.Length)
                            {
                                text.Append(", ");
                                text.Append(genericArguments[skipGenericArguments].GetNameCS(fullName));
                                skipGenericArguments++;
                            }
                        }

                        text.Append('>');
                    }
                }
            }

            return skipGenericArguments;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

