// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// <see cref="GUIContent"/> with <see cref="IDisposable"/> connected to an <see cref="ObjectPool{T}"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/PooledGUIContent
    public class PooledGUIContent : GUIContent,
        IDisposable
    {
        /************************************************************************************************************************/

        /// <summary><see cref="ObjectPool{T}.Acquire()"/>s and initializes an instance.</summary>
        public static PooledGUIContent Acquire(
            string text = null,
            string tooltip = null,
            Texture image = null)
        {
            var item = Pool.Instance.Acquire();
            item.text = text;
            item.tooltip = tooltip;
            item.image = image;
            return item;
        }

        /// <summary><see cref="ObjectPool{T}.Acquire()"/>s and initializes an instance.</summary>
        public static PooledGUIContent Acquire(
            UnityEditor.SerializedProperty property)
            => Acquire(property.displayName, property.tooltip);

        /************************************************************************************************************************/

        /// <summary><see cref="ObjectPool{T}.Release(T)"/>.</summary>
        public void Dispose()
            => Pool.Instance.Release(this);

        /************************************************************************************************************************/

        /// <summary>An <see cref="ObjectPool{T}"/> for <see cref="PooledGUIContent"/>.</summary>
        /// https://kybernetik.com.au/animancer/api/Animancer.Editor/Pool
        public class Pool : ObjectPool<PooledGUIContent>
        {
            /************************************************************************************************************************/

            /// <summary>Singleton instance.</summary>
            public static Pool Instance = new();

            /************************************************************************************************************************/

            /// <inheritdoc/>
            protected override PooledGUIContent New()
                => new();

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override PooledGUIContent Acquire()
            {
                var content = base.Acquire();

#if UNITY_ASSERTIONS
                if (!string.IsNullOrEmpty(content.text) ||
                    !string.IsNullOrEmpty(content.tooltip) ||
                    content.image != null)
                {
                    throw new UnityEngine.Assertions.AssertionException(
                        $"• {nameof(content.text)} = '{content.text}'" +
                        $"\n• {nameof(content.tooltip)} = '{content.tooltip}'" +
                        $"\n• {nameof(content.image)} = '{content.image}'",
                        $"A {nameof(PooledGUIContent)} is not cleared." + NotResetError);
                }
#endif

                return content;
            }

            /************************************************************************************************************************/

            /// <inheritdoc/>
            public override void Release(PooledGUIContent content)
            {
                content.text = null;
                content.tooltip = null;
                content.image = null;
                base.Release(content);
            }

            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
    }
}

#endif

