// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity
{
    /// <summary>
    /// Main class for implementing custom typewriter waits, to be assigned in the inspector
    /// </summary>
    /// <example>
    /// 0. import the necessary Febucci namespaces
    /// <code>
    /// using Febucci.TextAnimatorCore;
    /// using Febucci.TextAnimatorCore.Text;
    /// using Febucci.TextAnimatorForUnity;
    /// </code>
    /// 1. Create a scriptable object class that inherits from <see cref="TypingsTimingsScriptableBase"/>
    /// and contains custom wait logic
    /// <code>
    /// using UnityEngine;
    ///
    /// [System.Serializable] // --- remember to serialize your scriptables!
    /// [CreateAssetMenu(fileName = "Custom Typewriter Waits")]
    /// class CustomTypingWaits : TypingsTimingsScriptableBase
    /// {
    ///     // --- put your properties here as normal
    ///     [SerializeField] float delay = .1f;
    ///
    ///     // custom waits when showing text
    ///     public override float GetWaitAppearanceTimeOf(CharacterData character, TextAnimator animator)
    ///     {
    ///         // example: skips spaces
    ///         if (char.IsWhiteSpace(character.info.character))
    ///             return 0;
    ///
    ///         return delay;
    ///     }
    ///
    ///     // custom waits when disappearing text
    ///     public override float GetWaitDisappearanceTimeOf(CharacterData character, TextAnimator animator)
    ///     {
    ///         // in this case, it's the same as appearances
    ///         return GetWaitAppearanceTimeOf(character, animator);
    ///     }
    /// }
    /// </code>
    /// </example>
    [System.Serializable]
    public abstract class TypingsTimingsScriptableBase : ScriptableObject, ITypingTimingsProvider
    {
        public abstract float GetWaitAppearanceTimeOf(CharacterData character, TextAnimator animator);
        public virtual float GetWaitDisappearanceTimeOf(CharacterData character, TextAnimator animator)
            => GetWaitAppearanceTimeOf(character, animator);
    }
}