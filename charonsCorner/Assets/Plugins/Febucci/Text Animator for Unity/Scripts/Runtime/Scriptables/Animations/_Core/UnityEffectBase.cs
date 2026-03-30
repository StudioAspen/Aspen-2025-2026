// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore;
using Febucci.TextAnimatorCore.Text;
using Febucci.Parsing;

namespace Febucci.TextAnimatorForUnity
{
    public class UnityEffectBase : IEffectState
    {

        public void UpdateParameters(RegionParameters parameters)
        {
            throw new System.NotImplementedException();
        }

        public void Apply(ref CharacterData character, in ManagedEffectContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}