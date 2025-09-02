// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;

namespace Animancer
{
    /// <summary>An object that can be copied.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/ICopyable_1
    public interface ICopyable<in T>
    {
        /************************************************************************************************************************/

        /// <summary>Copies the contents of `copyFrom` into this object, replacing its previous contents.</summary>
        void CopyFrom(T copyFrom, CloneContext context);

        /************************************************************************************************************************/
    }

    /// <summary>Extension methods for <see cref="ICopyable{T}"/>.</summary>
    /// https://kybernetik.com.au/animancer/api/Animancer/CopyableExtensions
    public static partial class CopyableExtensions
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="ICopyable{T}.CopyFrom"/>
        /// using a <see cref="CloneContext"/> from the <see cref="CloneContext.Pool"/>.
        /// </summary>
        public static void CopyFrom<T>(this T copyTo, T copyFrom)
            where T : ICopyable<T>
        {
            var context = CloneContext.Pool.Instance.Acquire();
            copyTo.CopyFrom(copyFrom, context);
            CloneContext.Pool.Instance.Release(context);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a new <typeparamref name="T"/> and calls <see cref="ICopyable{T}.CopyFrom"/>.
        /// </summary>
        public static T CopyableClone<T>(this T original, CloneContext context)
            where T : ICopyable<T>
        {
            if (original == null)
                return default;

            var clone = (T)Activator.CreateInstance(original.GetType());
            clone.CopyFrom(original, context);
            return clone;
        }

        /// <summary>
        /// Creates a new <typeparamref name="T"/> and calls <see cref="ICopyable{T}.CopyFrom"/>
        /// using a <see cref="CloneContext"/> from the <see cref="CloneContext.Pool"/>.
        /// </summary>
        public static T CopyableClone<T>(this T original)
            where T : ICopyable<T>
        {
            var context = CloneContext.Pool.Instance.Acquire();
            var clone = original.CopyableClone(context);
            CloneContext.Pool.Instance.Release(context);
            return clone;
        }

        /************************************************************************************************************************/

        /// <summary>Calls <see cref="ICopyable{T}.CopyFrom"/> using the appropriate type.</summary>
        public static void CopyFromBase<TChild, TBase>(this TChild copyTo, TBase copyFrom, CloneContext context)
            where TChild : ICopyable<TChild>, ICopyable<TBase>
            where TBase : ICopyable<TBase>
        {
            if (copyFrom is TChild copyFromChild)
                copyTo.CopyFrom(copyFromChild, context);
            else
                copyTo.CopyFrom(copyFrom, context);
        }

        /************************************************************************************************************************/
    }
}

