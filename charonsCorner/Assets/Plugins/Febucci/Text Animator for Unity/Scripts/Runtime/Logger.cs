// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Diagnostics;

namespace Febucci.TextAnimatorForUnity
{

    public static class Logger
    {
        const string PREFIX = "[TextAnimatorForUnity] ";

        [Conditional("FEBUCCI_TEXT_ANIMATOR_DEBUG")]
        public static void Debug(string text, UnityEngine.Object context = null)
        {
            UnityEngine.Debug.Log(PREFIX + text, context);
        }

        [Conditional("FEBUCCI_TEXT_ANIMATOR_DEBUG")]
        public static void DebugError(string text, UnityEngine.Object context = null)
        {
            UnityEngine.Debug.LogError(PREFIX + text, context);
        }

        public static void Log(string text, UnityEngine.Object context = null) => UnityEngine.Debug.Log(PREFIX + text, context);
        public static void Warning(string text, UnityEngine.Object context = null) => UnityEngine.Debug.LogWarning(PREFIX + text, context);
        public static void Error(string text, UnityEngine.Object context = null) => UnityEngine.Debug.LogError(PREFIX + text, context);
    }

}