// =======================================================
// Text Animator for Unity - Copyright (c) 2018-Today, Febucci SRL, febucci.com
// - LICENSE: https://www.textanimatorforgames.com/legal/eula
// - DOCUMENTATION: https://docs.febucci.com/text-animator-unity/
// - WEBSITE: https://www.textanimatorforgames.com/
// =======================================================

using System;
using System.Collections.Generic;
using Febucci.TextAnimatorCore.Data;
using UnityEngine;
using Febucci.TextAnimatorCore.Styles;

namespace Febucci.TextAnimatorForUnity.Styles
{
    /// <summary>
    /// Contains a list of styles that can be used in the text
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(menuName = ScriptablePaths.STYLES_PATH+"StyleSheet", fileName = "StyleSheet for Text Animator")]
    public class StyleSheetScriptable : ScriptableObject, IDatabaseProvider<Style>
    {
        [SerializeField] Style[] styles = Array.Empty<Style>();

        public Dictionary<string, Style> Database
        {
            get
            {
                BuildOnce();
                return dictionary;
            }
        }

        public Style[] Styles
        {
            get => styles;
            set
            {
                styles = value;
                built = false;
            }
        }

        bool built;
        Dictionary<string, Style> dictionary;

        public void BuildOnce()
        {
            if (built) return;
            built = true;

            if(dictionary != null) dictionary.Clear();
            else dictionary = new Dictionary<string, Style>();

            if (styles == null) return;

            foreach (var style in styles)
            {
                var tag = style.styleTag.ToLowerInvariant();
                if (string.IsNullOrEmpty(tag)) continue;

                if (dictionary.ContainsKey(tag))
                {
                    Debug.LogError($"[TextAnimator] StyleSheetScriptable: duplicated style tag '{tag}", this);
                    continue;
                }

                dictionary.Add(tag, style);
            }
        }

        public void ForceBuildRefresh()
        {
            built = false;
            BuildOnce();
        }

        public virtual bool TryGetStyle(string tag, out Style result)
        {
            BuildOnce();
            return dictionary.TryGetValue(tag, out result);
        }

        #if UNITY_EDITOR
        void OnValidate()
        {
            built = false;
            if (!Application.isPlaying) return;
            BuildOnce();
        }
        #endif
    }
}