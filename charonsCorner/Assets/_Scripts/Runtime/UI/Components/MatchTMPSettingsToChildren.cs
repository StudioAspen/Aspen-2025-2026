using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CharonsCorner.Runtime
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(TMP_Text))]
    public class MatchTMPSettingsToChildren : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _recursive = true;
        [SerializeField] private bool _includeInactive = true;

        public void MatchSettings()
        {
            TMP_Text source = GetComponent<TMP_Text>();
            if (source == null)
            {
                Debug.LogError("[MatchTMPSettingsToChildren] Source TMP_Text component not found!");
                return;
            }

            TMP_Text[] children;
            if (_recursive)
            {
                children = GetComponentsInChildren<TMP_Text>(_includeInactive);
            }
            else
            {
                int childCount = transform.childCount;
                System.Collections.Generic.List<TMP_Text> list = new System.Collections.Generic.List<TMP_Text>();
                for (int i = 0; i < childCount; i++)
                {
                    TMP_Text childTmp = transform.GetChild(i).GetComponent<TMP_Text>();
                    if (childTmp != null) list.Add(childTmp);
                }
                children = list.ToArray();
            }

            int count = 0;
            foreach (var target in children)
            {
                // Skip the source itself
                if (target == source) continue;

#if UNITY_EDITOR
                Undo.RecordObject(target, "Match TMP Settings");
#endif
                
                // Core Settings
                target.font = source.font;
                target.fontSharedMaterial = source.fontSharedMaterial;
                target.fontSize = source.fontSize;
                target.fontSizeMin = source.fontSizeMin;
                target.fontSizeMax = source.fontSizeMax;
                target.enableAutoSizing = source.enableAutoSizing;
                
                target.fontStyle = source.fontStyle;
                target.color = source.color;
                target.enableVertexGradient = source.enableVertexGradient;
                target.colorGradient = source.colorGradient;
                target.colorGradientPreset = source.colorGradientPreset;
                
                // Character Settings
                target.characterSpacing = source.characterSpacing;
                target.wordSpacing = source.wordSpacing;
                target.lineSpacing = source.lineSpacing;
                target.paragraphSpacing = source.paragraphSpacing;
                
                // Extra Settings
                target.enableWordWrapping = source.enableWordWrapping;
                target.overflowMode = source.overflowMode;
                target.margin = source.margin;
                target.raycastTarget = source.raycastTarget;
                target.maskable = source.maskable;
                
                // Rich Text
                target.richText = source.richText;
                target.parseCtrlCharacters = source.parseCtrlCharacters;
                
                // Geometry
                target.geometrySortingOrder = source.geometrySortingOrder;
                
                // We EXCLUDE:
                // target.text
                // target.alignment

#if UNITY_EDITOR
                EditorUtility.SetDirty(target);
#endif
                count++;
            }

            Debug.Log($"[MatchTMPSettingsToChildren] Successfully matched settings to {count} children.");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MatchTMPSettingsToChildren))]
    public class MatchTMPSettingsToChildrenEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MatchTMPSettingsToChildren script = (MatchTMPSettingsToChildren)target;

            GUILayout.Space(10);
            if (GUILayout.Button("Match TMP Settings to Children", GUILayout.Height(30)))
            {
                script.MatchSettings();
            }
        }
    }
#endif
}
