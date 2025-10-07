// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Animancer
{
    /// <summary>
    /// A <see cref="string"/> wrapper which allows fast reference equality checks and dictionary usage
    /// by ensuring that users of identical strings are given the same <see cref="StringReference"/>
    /// instead of needing to compare each character in the strings.
    /// </summary>
    /// <remarks>
    /// Rather than a constructor, instances of this class are acquired via <see cref="Get(string)"/>
    /// or via implicit conversion from <see cref="string"/> (which calls the same method).
    /// <para></para>
    /// Unlike <c>UnityEngine.InputSystem.Utilities.InternedString</c>,
    /// this implementation is case-sensitive and treats <c>null</c> and <c>""</c> as not equal.
    /// It's also a class to allow usage as a key in a dictionary keyed by <see cref="object"/> without boxing.
    /// <para></para>
    /// <strong>Example:</strong>
    /// <code>
    /// public static readonly StringReference MyStringReference = "My String";
    /// </code>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/StringReference
    public class StringReference :
        IComparable<StringReference>,
        IConvertable<string>
    {
        /************************************************************************************************************************/

        /// <summary>The encapsulated <see cref="string"/>.</summary>
        /// <remarks>This field will never be null.</remarks>
        public readonly string String;

        /************************************************************************************************************************/

        private static readonly Dictionary<string, StringReference>
            StringToReference = new(256);

        /// <summary>Returns a <see cref="StringReference"/> containing the `value`.</summary>
        /// <remarks>
        /// The returned reference is cached and the same one will be
        /// returned each time this method is called with the same `value`.
        /// <para></para>
        /// Returns <c>null</c> if the `value` is <c>null</c>.
        /// <para></para>
        /// The `value` is case sensitive.
        /// </remarks>
        public static StringReference Get(string value)
        {
            if (value is null)
                return null;

            if (!StringToReference.TryGetValue(value, out var reference))
                StringToReference.Add(value, reference = new(value));

            // This system could be made case insensitive based on a static bool.
            // If true, convert the value to lower case for the dictionary key but still reference the original.
            // When changing the setting, rebuild the dictionary with the appropriate keys.

            return reference;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new array of <see cref="StringReference"/>s to the `strings`.</summary>
        public static StringReference[] Get(params string[] strings)
        {
            if (strings == null)
                return null;

            if (strings.Length == 0)
                return Array.Empty<StringReference>();

            var references = new StringReference[strings.Length];
            for (int i = 0; i < strings.Length; i++)
                references[i] = strings[i];
            return references;
        }

        /************************************************************************************************************************/

        /// <summary>Returns a <see cref="StringReference"/> containing the `value` if one has already been created.</summary>
        /// <remarks>The `value` is case sensitive.</remarks>
        public static bool TryGet(string value, out StringReference reference)
        {
            if (value is not null && StringToReference.TryGetValue(value, out reference))
                return true;

            reference = null;
            return false;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="StringReference"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private StringReference(string value)
            => String = value;

        /// <summary>Calls <see cref="Get(string)"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator StringReference(string value)
            => Get(value);

        /// <summary>[Internal]
        /// Returns a new <see cref="StringReference"/> which will not be shared by regular calls to
        /// <see cref="Get(string)"/>.
        /// </summary>
        /// <remarks>
        /// This means the reference will never be equal to others
        /// even if they contain the same <see cref="String"/>.
        /// </remarks>
        internal static StringReference Unique(string value)
            => new(value);

        /************************************************************************************************************************/

        /// <summary>Returns the <see cref="String"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
            => String;

        /// <summary>Returns the <see cref="String"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(StringReference value)
            => value?.String;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        string IConvertable<string>.Convert()
            => String;

        /************************************************************************************************************************/

        /// <summary>Compares the <see cref="String"/>s.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(StringReference other)
            => String.CompareTo(other?.String);

        /************************************************************************************************************************/
    }

    /// <summary>Extension methods for <see cref="StringReference"/>.</summary>
    public static class StringReferenceExtensions
    {
        /************************************************************************************************************************/

        /// <summary>Is the `reference` <c>null</c> or its <see cref="StringReference.String"/> empty?</summary>
        /// <remarks>Similar to <see cref="string.IsNullOrEmpty"/>.</remarks>
        public static bool IsNullOrEmpty(this StringReference reference)
            => reference is null
            || reference.String.Length == 0;

        /************************************************************************************************************************/

        /// <summary>
        /// Is the <see cref="StringReference.String"/> equal to the `other`
        /// when treating <c>""</c> as equal to <c>null</c>?
        /// </summary>
        public static bool EqualsWhereEmptyIsNull(this StringReference reference, StringReference other)
        {
            if (reference == other)
                return true;
            else if (reference == null)
                return other.String.Length == 0;
            else if (reference.String.Length == 0)
                return other == null;
            else
                return false;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new array containing the <see cref="StringReference.String"/>s.</summary>
        public static string[] ToStrings(this StringReference[] references)
        {
            if (references == null)
                return null;

            if (references.Length == 0)
                return Array.Empty<string>();

            var strings = new string[references.Length];
            for (int i = 0; i < references.Length; i++)
                strings[i] = references[i];
            return strings;
        }

        /************************************************************************************************************************/
    }
}

