// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System.Collections.Generic;
using Febucci.TextAnimatorCore.Data;
using Febucci.TextAnimatorCore.Typing;
using Febucci.TextAnimatorForUnity.Actions.Core;

namespace Febucci.TextAnimatorForUnity.Actions
{
    [System.Serializable]
    [UnityEngine.CreateAssetMenu(fileName = "ActionDatabase", menuName = ScriptablePaths.ACTIONS_PATH + "Create Actions Database", order = 100)]
    public class ActionDatabase : TextAnimatorForUnity.Database<ActionScriptableBase>, IDatabaseProvider<ITypewriterAction>
    {

        public override bool IsCaseSensitive => false;

        Dictionary<string, ITypewriterAction> converted = new Dictionary<string, ITypewriterAction>();
        public Dictionary<string, ITypewriterAction> Database
        {
            get
            {
                BuildOnce();
                return converted;
            }
        }

        protected override void OnBuildOnce()
        {
            base.OnBuildOnce();
            converted = new Dictionary<string, ITypewriterAction>();
            foreach (var pair in base.Dictionary)
                converted.Add(pair.Key, pair.Value);
        }
    }
}