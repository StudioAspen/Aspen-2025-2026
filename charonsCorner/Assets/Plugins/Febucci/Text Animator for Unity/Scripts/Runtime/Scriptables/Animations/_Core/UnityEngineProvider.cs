// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Time;

namespace Febucci.TextAnimatorForUnity
{
    public class UnityEngineProvider : IEngineProvider
    {
        public float GetCurrentDeltaTime(TimeScale scale) => scale == TimeScale.Scaled ? UnityEngine.Time.deltaTime : UnityEngine.Time.unscaledDeltaTime;

        public readonly static UnityEngineProvider Instance = new UnityEngineProvider();
    }
}