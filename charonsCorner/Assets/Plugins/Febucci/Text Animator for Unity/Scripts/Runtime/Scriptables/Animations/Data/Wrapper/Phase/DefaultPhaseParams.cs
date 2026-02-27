// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using UnityEngine;
namespace Febucci.TextAnimatorForUnity.Effects
{
    [System.Serializable]
    public class DefaultPhaseParams
    {
        [Range(0, 1)] public float charOffset;
        [Range(0, 1)] public float wordOffset;
        public float speed = 1f;
    }
}