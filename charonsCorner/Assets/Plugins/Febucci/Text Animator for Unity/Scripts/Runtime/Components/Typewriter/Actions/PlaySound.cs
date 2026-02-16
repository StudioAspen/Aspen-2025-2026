// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Collections;
using Febucci.TextAnimatorCore.Typing;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Actions
{
    [AddComponentMenu(ScriptablePaths.ACTIONS_PATH+"Play Sound Action")]
    sealed class PlaySound : TypewriterActionComponent
    {
        [SerializeField] AudioSource source;

        protected override IEnumerator PerformAction(TypingInfo typingInfo)
        {
            if (source != null && source.clip != null)
            {
                source.Play();
                yield return new WaitForSeconds(source.clip.length);
            }
        }
    }
}