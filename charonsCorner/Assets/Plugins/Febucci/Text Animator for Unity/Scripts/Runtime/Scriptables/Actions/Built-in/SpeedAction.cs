// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorCore.Typing.BuiltIn;
using Febucci.TextAnimatorForUnity.Actions.Core;

using UnityEngine;

namespace Febucci.TextAnimatorForUnity.Actions
{
    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.ACTIONS_PATH+"Speed", fileName = "Speed Action")]
    [TagInfo("speed")]
    sealed class SpeedAction : CoreLibraryActionScriptableWrapper<SpeedActionState>
    {
        /// <summary>
        /// Speed used in case the action does not have the first parameter
        /// </summary>
        [UnityEngine.Tooltip("Speed used in case the action does not have the first parameter")]
        public float defaultSpeed = 2;

        protected override SpeedActionState CreateState(ActionMarker marker, object typewriter)
        {
            // Parse speed multiplier from first parameter (default: 1.0)
            float speedMultiplier = 1f;
            if (marker.parameters != null && marker.parameters.Length > 0)
            {
                if (!float.TryParse(marker.parameters[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out speedMultiplier))
                {
                    speedMultiplier =defaultSpeed;
                }
            }

            return new SpeedActionState(speedMultiplier);
        }
    }
}