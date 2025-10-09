// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System.Text;

namespace Animancer
{
    /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="StringBuilder"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/StringBuilderPool
    public class StringBuilderPool : ObjectPool<StringBuilder>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton.</summary>
        public static StringBuilderPool Instance = new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override StringBuilder New()
            => new();

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override StringBuilder Acquire()
        {
            var text = base.Acquire();
            AnimancerUtilities.Assert(
                text.Length == 0,
                $"A pooled {nameof(StringBuilder)} is not empty." + NotResetError);
            return text;
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override void Release(StringBuilder text)
        {
            text.Length = 0;
            base.Release(text);
        }

        /************************************************************************************************************************/
    }

    /// https://kybernetik.com.au/animancer/api/Animancer/AnimancerUtilities
    public static partial class AnimancerUtilities
    {
        /************************************************************************************************************************/

        /// <summary>[Animancer Extension]
        /// Calls <see cref="StringBuilder.ToString()"/> and <see cref="StringBuilderPool.Release(StringBuilder)"/>.
        /// </summary>
        public static string ReleaseToString(this StringBuilder text)
        {
            var result = text.ToString();
            StringBuilderPool.Instance.Release(text);
            return result;
        }

        /************************************************************************************************************************/
    }
}

