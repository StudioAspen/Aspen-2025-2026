// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Text;

namespace Animancer
{
    /// <summary>An object which can give a detailed description of itself.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/IHasDescription
    /// 
    public interface IHasDescription
    {
        /************************************************************************************************************************/

        /// <summary>Appends a detailed descrption of the current details of this object.</summary>
        /// <remarks>
        /// <see cref="AnimancerUtilities.GetDescription"/> calls this method with a pooled
        /// <see cref="StringBuilder"/>.
        /// </remarks>
        void AppendDescription(StringBuilder text, string separator = "\n");

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    /// 
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="IHasDescription.AppendDescription"/> with a pooled <see cref="StringBuilder"/>.
        /// </summary>
        public static string GetDescription(
            this IHasDescription hasDescription,
            string separator = "\n")
        {
            if (hasDescription == null)
                return "Null";

            var text = StringBuilderPool.Instance.Acquire();
            hasDescription.AppendDescription(text, separator);
            return text.ReleaseToString();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Appends "Null" if `maybeHasDescription` is null. Otherwise calls
        /// <see cref="IHasDescription.AppendDescription"/>.
        /// </summary>
        public static StringBuilder AppendDescription<T>(
            this StringBuilder text,
            T maybeHasDescription,
            string separator = "\n",
            bool fullNodeDescription = false)
            => maybeHasDescription is IHasDescription hasDescription
            ? text.AppendDescription(hasDescription, separator, fullNodeDescription)
            : text.Append(ToStringOrNull(maybeHasDescription));

        /// <summary>
        /// Appends "Null" if `hasDescription` is null. Otherwise calls
        /// <see cref="IHasDescription.AppendDescription"/>.
        /// </summary>
        public static StringBuilder AppendDescription(
            this StringBuilder text,
            IHasDescription hasDescription,
            string separator = "\n",
            bool fullNodeDescription = false)
        {
            if (hasDescription == null)
                return text.Append("Null");

            if (!fullNodeDescription && hasDescription is AnimancerNode node)
                return text.Append(node.GetPath());

            hasDescription.AppendDescription(text, separator);
            return text;
        }

        /************************************************************************************************************************/

        /// <summary>Appends <c>{prefix}{name}: {value}</c>.</summary>
        public static StringBuilder AppendField<T>(
            this StringBuilder text,
            string prefix,
            string name,
            T value,
            string separator = "\n",
            bool fullNodeDescription = false)
            => text
            .Append(prefix)
            .Append(name)
            .Append(": ")
            .AppendDescription(value, separator, fullNodeDescription);

        /************************************************************************************************************************/

        /// <summary>Does the `text` start with a new line character?</summary>
        public static bool StartsWithNewLine(this string text)
        {
            if (text == null || text.Length == 0)
                return false;

            var start = text[0];
            return start == '\n' || start == '\r';
        }

        /************************************************************************************************************************/
    }
}

