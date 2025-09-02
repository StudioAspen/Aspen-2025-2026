// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using UnityEngine;

namespace Animancer
{
    /// <summary>[Assert-Conditional]
    /// A <see cref="HelpURLAttribute"/> which points to Animancer's documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional(Strings.Assertions)]
    public class AnimancerHelpUrlAttribute : HelpURLAttribute
    {
        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerHelpUrlAttribute"/>.</summary>
        public AnimancerHelpUrlAttribute(string url)
            : base(url)
        { }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimancerHelpUrlAttribute"/>.</summary>
        public AnimancerHelpUrlAttribute(Type type)
            : base(GetApiDocumentationUrl(type))
        { }

        /************************************************************************************************************************/

        /// <summary>Returns a URL for the given `type`'s API Documentation page.</summary>
        public static string GetApiDocumentationUrl(Type type)
            => GetApiDocumentationUrl(Strings.DocsURLs.Documentation + "/api/", type);

        /// <summary>Returns a URL for the given `type`'s API Documentation page.</summary>
        public static string GetApiDocumentationUrl(string prefix, Type type)
        {
            var url = StringBuilderPool.Instance.Acquire();

            url.Append(prefix);

            if (!string.IsNullOrEmpty(type.Namespace))
                url.Append(type.Namespace).Append('/');

            url.Append(type.Name.Replace('`', '_'));

            return url.ReleaseToString();
        }

        /************************************************************************************************************************/
    }
}

