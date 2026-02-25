// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;

namespace Febucci.TextAnimatorForUnity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DefaultValueAttribute : Attribute
    {
        public readonly string variableName;
        public readonly float variableValue;

        public DefaultValueAttribute(string variableName, float variableValue)
        {
            this.variableName = variableName;
            this.variableValue = variableValue;
        }
    }
}