// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorForUnity.Actions;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity
{
    // TODO future version
    internal static class TypewriterExtensions
    {

        public static TypewriterComponent InstantiateTypewriter(this ITextAnimatorProvider animatorProvider, ActionDatabase database, TypingsTimingsScriptableBase typewriterSettings)
        {
            var go = new GameObject($"Typewriter");
            var typewriter = go.AddComponent<TypewriterComponent>();
            if (typewriter == null)
            {
                Debug.LogError("Error attaching typewriter");
                return null;
            }

            typewriter.AssignAnimator(animatorProvider, database, typewriterSettings);
            return typewriter;
        }
    }
}