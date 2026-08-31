using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CharonsCorner.Runtime
{
    public enum Speaker
    {
        Charon,
        Bowley,
        Unknown, // Mapping ??? to Unknown in code for better naming, but will display as ??? in inspector if possible or just use string mapping
        LostMemento
    }

    [System.Serializable]
    public struct DialogueLine
    {
        [HideLabel]
        public Speaker speaker;

        [ValueDropdown("GetCharonAnimations")]
        public string charonAnimation;

        [ValueDropdown("GetCharonStareAnimations")]
        public string charonStareAnimation;

        [ValueDropdown("GetBowleyAnimations")]
        public string bowleyAnimation;

        [HideLabel]
        [TextArea(3, 10)]
        public string text;

#if UNITY_EDITOR
        private static IEnumerable GetCharonAnimations()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:CharonAnimationStrings");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<CharonAnimationStrings>(path);
                if (so != null) return so.AnimationEvents;
            }
            return new List<string>();
        }

        private static IEnumerable GetCharonStareAnimations()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:CharonStareAnimationStrings");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<CharonStareAnimationStrings>(path);
                if (so != null) return so.AnimationEvents;
            }
            return new List<string>();
        }

        private static IEnumerable GetBowleyAnimations()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:BowleyAnimationStrings");
            if (guids.Length > 0)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<BowleyAnimationStrings>(path);
                if (so != null) return so.AnimationEvents;
            }
            return new List<string>();
        }
#endif
    }

    [CreateAssetMenu(fileName = "DialogueSequence", menuName = "CharonsCorner/Dialogue/Dialogue Sequence", order = 2)]
    public class DialogueSequenceSO : ScriptableObject
    {
        [field: SerializeField] public string SequenceName { get; private set; } = string.Empty;
        public DialogueLine[] lines;
    }
}
