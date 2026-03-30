// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using Febucci.TextAnimatorCore.Settings;
using UnityEngine;

namespace Febucci.TextAnimatorForUnity
{
    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.SETTINGS_PATH+"Typewriter Settings", fileName = "Typewriter Settings for Text Animator")]
    public class TypewriterSettingsScriptable : ScriptableObject, ISettingsProvider<UnityTypewriterSettings>
    {
        [SerializeField] public UnityTypewriterSettings settings = new UnityTypewriterSettings();
        public UnityTypewriterSettings Settings => settings;
    }
}